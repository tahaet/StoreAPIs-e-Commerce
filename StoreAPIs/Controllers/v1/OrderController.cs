using System.Diagnostics;
using System.Numerics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using StoreDataAccess.Repository.IRepository;
using StoreModels;
using StoreModels.Dtos;
using StoreUtility;
using Stripe;
using Stripe.Checkout;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class OrderController : Controller
    {
        private readonly IOrderHeaderRepository orderHeaderRepository;
        private readonly IOrderDetailRepository orderDetailRepository;
        private readonly ILogger<OrderController> logger;

        [BindProperty]
        public OrderDto OrderDto { get; set; }

        public OrderController(
            IOrderHeaderRepository orderHeaderRepository,
            IOrderDetailRepository orderDetailRepository,
            ILogger<OrderController> logger
        )
        {
            this.orderHeaderRepository = orderHeaderRepository;
            this.orderDetailRepository = orderDetailRepository;
            this.logger = logger;
        }

        [HttpGet("allorders")]
        public async Task<ActionResult<List<OrderHeader>>> GetAll(string status)
        {
            try
            {
                IEnumerable<OrderHeader> orderHeaders;

                if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
                {
                    orderHeaders = await orderHeaderRepository.GetAll(
                        includeProperties: "ApplicationUser"
                    );
                }
                else
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    orderHeaders = await orderHeaderRepository.GetAll(
                        u => u.ApplicationUserId == claim.Value,
                        includeProperties: "ApplicationUser"
                    );
                }

                switch (status)
                {
                    case "pending":
                        orderHeaders = orderHeaders.Where(u =>
                            u.PaymentStatus == SD.PaymentStatusDelayedPayment
                        );
                        break;
                    case "inprocess":
                        orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                        break;
                    case "completed":
                        orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                        break;
                    case "approved":
                        orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                        break;
                    default:
                        break;
                }

                return Ok(orderHeaders);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("details/{orderId:int}")]
        public async Task<ActionResult<OrderDto>> Details(int orderId)
        {
            try
            {
                OrderDto = new OrderDto()
                {
                    OrderHeader = await orderHeaderRepository.Get(
                        u => u.Id == orderId,
                        includeProperties: "ApplicationUser"
                    ),
                    OrderDetails = await orderDetailRepository.GetAll(
                        u => u.OrderHeaderId == orderId,
                        includeProperties: "Product"
                    ),
                };
                return Ok(OrderDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("Details")]
        public async Task<ActionResult<StripeRequest>> Details_PAY_NOW(
            StripeRequestDto stripeRequestDto
        )
        {
            try
            {
                OrderDto.OrderHeader = await orderHeaderRepository.Get(
                    u => u.Id == OrderDto.OrderHeader.Id,
                    includeProperties: "ApplicationUser"
                );
                OrderDto.OrderDetails = await orderDetailRepository.GetAll(
                    u => u.OrderHeaderId == OrderDto.OrderHeader.Id,
                    includeProperties: "Product"
                );

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card", },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = stripeRequestDto.ApprovedUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                };

                foreach (var item in OrderDto.OrderDetails)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), //20.00 -> 2000
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name
                            },
                        },
                        Quantity = item.Count,
                    };

                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                orderHeaderRepository.UpdateStripePaymentId(
                    OrderDto.OrderHeader.Id,
                    session.Id,
                    session.PaymentIntentId
                );
                await orderHeaderRepository.Save();
                stripeRequestDto.StripeSessionUrl = session.Url;
                return Ok(stripeRequestDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("PaymentConfirmation/{orderHeaderId:int}")]
        public async Task<ActionResult<int>> PaymentConfirmation(int orderHeaderId)
        {
            try
            {
                OrderHeader orderHeader = await orderHeaderRepository.Get(u =>
                    u.Id == orderHeaderId
                );
                if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
                {
                    var service = new SessionService();
                    Session session = service.Get(orderHeader.SessionId);
                    //check the stripe status
                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        orderHeaderRepository.UpdateStatus(
                            orderHeaderId,
                            orderHeader.OrderStatus,
                            SD.PaymentStatusApproved
                        );
                        orderHeaderRepository.Save();
                    }
                }
                return Ok(orderHeaderId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<int>> UpdateOrderDetail(int orderHeaderId)
        {
            try
            {
                var orderHEaderFromDb = await orderHeaderRepository.Get(
                    u => u.Id == orderHeaderId,
                    tracked: false
                );
                if (orderHEaderFromDb is null)
                    return NotFound();

                orderHEaderFromDb.Name = OrderDto.OrderHeader.Name;
                orderHEaderFromDb.PhoneNumber = OrderDto.OrderHeader.PhoneNumber;
                orderHEaderFromDb.StreetAddress = OrderDto.OrderHeader.StreetAddress;
                orderHEaderFromDb.City = OrderDto.OrderHeader.City;
                orderHEaderFromDb.State = OrderDto.OrderHeader.State;
                orderHEaderFromDb.PostalCode = OrderDto.OrderHeader.PostalCode;

                if (OrderDto.OrderHeader.Carrier != null)
                {
                    orderHEaderFromDb.Carrier = OrderDto.OrderHeader.Carrier;
                }
                if (OrderDto.OrderHeader.TrackingNumber != null)
                {
                    orderHEaderFromDb.TrackingNumber = OrderDto.OrderHeader.TrackingNumber;
                }
                orderHeaderRepository.Update(orderHEaderFromDb);
                await orderHeaderRepository.Save();
                return Ok(orderHEaderFromDb.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("StartProcessing")]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<int>> StartProcessing()
        {
            try
            {
                orderHeaderRepository.UpdateStatus(
                    OrderDto.OrderHeader.Id,
                    SD.StatusInProcess,
                    OrderDto.OrderHeader.PaymentStatus
                );
                await orderHeaderRepository.Save();
                return Ok(OrderDto.OrderHeader.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("ShipOrder")]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<int>> ShipOrder()
        {
            try
            {
                var orderHeader = await orderHeaderRepository.Get(
                    u => u.Id == OrderDto.OrderHeader.Id,
                    tracked: false
                );
                orderHeader.TrackingNumber = OrderDto.OrderHeader.TrackingNumber;
                orderHeader.Carrier = OrderDto.OrderHeader.Carrier;
                orderHeader.OrderStatus = SD.StatusShipped;
                orderHeader.ShippingDate = DateTime.Now;
                if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
                {
                    orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
                }
                orderHeaderRepository.Update(orderHeader);
                await orderHeaderRepository.Save();
                return Ok(OrderDto.OrderHeader.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("CancelOrder")]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<int>> CancelOrder()
        {
            try
            {
                var orderHeader = await orderHeaderRepository.Get(
                    u => u.Id == OrderDto.OrderHeader.Id,
                    tracked: false
                );
                if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
                {
                    var options = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = orderHeader.PaymentIntentId
                    };

                    var service = new RefundService();
                    Refund refund = service.Create(options);

                    orderHeaderRepository.UpdateStatus(
                        orderHeader.Id,
                        SD.StatusCancelled,
                        SD.StatusRefunded
                    );
                }
                else
                {
                    orderHeaderRepository.UpdateStatus(
                        orderHeader.Id,
                        SD.StatusCancelled,
                        SD.StatusCancelled
                    );
                }
                orderHeaderRepository.Save();

                return Ok(OrderDto.OrderHeader.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
