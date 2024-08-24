using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreDataAccess.Repository.IRepository;
using StoreModels;
using StoreModels.Dtos;
using StoreUtility;
using Stripe;
using Stripe.Checkout;

namespace StoreAPIs.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class CartController : Controller
    {
        private readonly IShoppingCartRepository shoppingCartRepository;
        private readonly IApplicationUserRepository applicationUserRepository;
        private readonly IOrderHeaderRepository orderHeaderRepository;
        private readonly IOrderDetailRepository orderDetailRepository;
        private readonly ILogger<CartController> logger;

        public CartController(
            IShoppingCartRepository shoppingCartRepository,
            ILogger<CartController> logger,
            IApplicationUserRepository applicationUserRepository,
            IOrderHeaderRepository orderHeaderRepository,
            IOrderDetailRepository orderDetailRepository
        )
        {
            this.shoppingCartRepository = shoppingCartRepository;
            this.logger = logger;
            this.applicationUserRepository = applicationUserRepository;
            this.orderHeaderRepository = orderHeaderRepository;
            this.orderDetailRepository = orderDetailRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShoppingCartDto>>> GetAllCarts()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            try
            {
                var carts = await shoppingCartRepository.GetAll(x => x.ApplicationUserId == userId);
                if (carts is null)
                    return NotFound();
                return Ok(carts);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShoppingCartDto>>> GetCartPerUserAndProduct([FromQuery] string userId, [FromQuery] int productId)
        {
           
            try
            {
                var cart = await shoppingCartRepository.Get(x => x.ApplicationUserId == userId && x.ProductId == productId);
                if (cart is null)
                    return NotFound();
                return Ok(cart);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }


        [HttpGet("{cartId:int}")]
        public async Task<ActionResult<IEnumerable<ShoppingCartDto>>> GetAllCarts(int cartId)
        {
           

            try
            {
                var cart = await shoppingCartRepository.Get(x => x.Id== cartId);
                if (cart is null)
                    return NotFound();
                return Ok(cart);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }


        [HttpPost("plus")]
        public async Task<IActionResult> plus(int cartId)
        {
            try
            {
                var cart = await shoppingCartRepository.Get(u => u.Id == cartId);
                if (cart is null)
                    return NotFound();

                cart.Count++;
                shoppingCartRepository.Update(cart);
                await shoppingCartRepository.Save();
                return Ok();
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("minus")]
        public async Task<IActionResult> minus(int cartId)
        {
            try
            {
                var cart = await shoppingCartRepository.Get(u => u.Id == cartId);
                if (cart is null)
                    return NotFound();

                HttpContext.Session.SetInt32(
                    SD.SessionCart,
                    shoppingCartRepository
                        .GetAll(u => u.ApplicationUserId == cart.ApplicationUserId)
                        .Result.Count() - 1
                );
                if (cart.Count <= 1)
                {
                    shoppingCartRepository.Remove(cart);
                }
                else
                {
                    cart.Count--;
                    shoppingCartRepository.Update(cart);
                }
                await shoppingCartRepository.Save();
                return Ok();
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Remove(int cartId)
        {
            try
            {
                var cart = await shoppingCartRepository.Get(u => u.Id == cartId);
                if (cart is null)
                    return NotFound();
                HttpContext.Session.SetInt32(
                    SD.SessionCart,
                    shoppingCartRepository
                        .GetAll(u => u.ApplicationUserId == cart.ApplicationUserId)
                        .Result.Count() - 1
                );
                shoppingCartRepository.Remove(cart);
                await shoppingCartRepository.Save();
                return Ok();
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(ShoppingCart shoppingCart)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    await shoppingCartRepository.Add(shoppingCart);
                    await shoppingCartRepository.Save();
                    return Ok();
                }
                return BadRequest();
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }



        [HttpGet("summary")]
        public async Task<ActionResult<ShoppingCartDto>> Summary()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                ShoppingCartDto shoppingCartDto =
                    new()
                    {
                        ShoppingCarts = await shoppingCartRepository.GetAll(
                            s => s.ApplicationUserId == userId,
                            includeProperties: "Product"
                        ),
                        OrderHeader = new OrderHeader()
                    };
                shoppingCartDto.OrderHeader.ApplicationUser = await applicationUserRepository.Get(
                    u => u.Id == userId
                );
                shoppingCartDto.OrderHeader.Name = shoppingCartDto.OrderHeader.ApplicationUser.Name;
                shoppingCartDto.OrderHeader.PhoneNumber = shoppingCartDto
                    .OrderHeader
                    .ApplicationUser
                    .PhoneNumber!;
                shoppingCartDto.OrderHeader.StreetAddress = shoppingCartDto
                    .OrderHeader
                    .ApplicationUser
                    .StreetAddress!;
                shoppingCartDto.OrderHeader.City = shoppingCartDto
                    .OrderHeader
                    .ApplicationUser
                    .City!;
                shoppingCartDto.OrderHeader.State = shoppingCartDto
                    .OrderHeader
                    .ApplicationUser
                    .State!;
                shoppingCartDto.OrderHeader.PostalCode = shoppingCartDto
                    .OrderHeader
                    .ApplicationUser
                    .PostalCode!;

                foreach (var cart in shoppingCartDto.ShoppingCarts)
                {
                    cart.Price = GetQuantatityPrice(cart);
                    shoppingCartDto.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }
                return Ok(shoppingCartDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("summary")]
        public async Task<IActionResult> SummaryPost([FromBody] StripeRequestDto stripeRequestDto)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartDto shoppingCartDto = new();
            try
            {
                shoppingCartDto.ShoppingCarts = await shoppingCartRepository.GetAll(
                    s => s.ApplicationUserId == userId,
                    includeProperties: "Product"
                );
                shoppingCartDto.OrderHeader.OrderDate = DateTime.Now;
                shoppingCartDto.OrderHeader.ApplicationUserId = userId;
                ApplicationUser applicationUser = await applicationUserRepository.Get(u =>
                    u.Id == userId
                );
                foreach (var cart in shoppingCartDto.ShoppingCarts)
                {
                    cart.Price = GetQuantatityPrice(cart);
                    shoppingCartDto.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }
                if (applicationUser.CompanyId.GetValueOrDefault() == 0)
                {
                    shoppingCartDto.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                    shoppingCartDto.OrderHeader.OrderStatus = SD.StatusPending;
                }
                else
                {
                    shoppingCartDto.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                    shoppingCartDto.OrderHeader.OrderStatus = SD.StatusApproved;
                }
                await orderHeaderRepository.Add(shoppingCartDto.OrderHeader);
                await orderHeaderRepository.Save();
                foreach (var cart in shoppingCartDto.ShoppingCarts)
                {
                    OrderDetail orderDetail =
                        new()
                        {
                            ProductId = cart.ProductId,
                            OrderHeaderId = shoppingCartDto.OrderHeader.Id,
                            Price = cart.Price,
                            Count = cart.Count
                        };
                    await orderDetailRepository.Add(orderDetail);
                    await orderDetailRepository.Save();
                }
                if (applicationUser.CompanyId.GetValueOrDefault() == 0)
                {
                    //stripe logic
                    var domain = $"{Request.Scheme}://{Request.Host.Value}/";

                    var options = new Stripe.Checkout.SessionCreateOptions
                    {
                        SuccessUrl = stripeRequestDto.ApprovedUrl,
                        CancelUrl = stripeRequestDto.CancelUrl,
                        LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                        Mode = "payment",
                    };
                    foreach (var item in shoppingCartDto.ShoppingCarts)
                    {
                        var sessionLineItem = new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(item.Price * 100),
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.Product.Name
                                }
                            },
                            Quantity = item.Count
                        };
                        options.LineItems.Add(sessionLineItem);
                    }
                    var discountsObj = new List<SessionDiscountOptions>()
                    {
                        new SessionDiscountOptions()
                        {
                            Coupon = stripeRequestDto.ShoppingCartDto.OrderHeader.CouponCode
                        }
                    };
                    if (stripeRequestDto.ShoppingCartDto.OrderHeader.Discount > 0)
                    {
                        options.Discounts = discountsObj;
                    }
                    var service = new SessionService();
                    Session session = service.Create(options);
                    orderHeaderRepository.UpdateStripePaymentId(
                        shoppingCartDto.OrderHeader.Id,
                        session.Id,
                        session.PaymentIntentId
                    );
                    await orderHeaderRepository.Save();
                    stripeRequestDto.StripeSessionUrl = session.Url;
                    return Ok(stripeRequestDto);
                }
                else
                {
                    //company user
                    return Ok(shoppingCartDto.OrderHeader.Id);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("ApplyCoupon")]
        public async Task<IActionResult> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartFromDb = await shoppingCartRepository.Get(u =>
                    u.ApplicationUserId == cartDto.CartHeader.UserId
                );
                cartFromDb.CouponCode = cartDto.CartHeader.CouponCode;
                shoppingCartRepository.Update(cartFromDb);
                await shoppingCartRepository.Save();
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpGet("confirm")]
        public async Task<ActionResult<int>> OrderConfirmation(int id)
        {
            try
            {
                OrderHeader orderHeader = await orderHeaderRepository.Get(
                    o => o.Id == id,
                    includeProperties: "ApplicationUser"
                );
                if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
                {
                    var service = new SessionService();
                    Session session = service.Get(orderHeader.SessionId);
                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        orderHeaderRepository.UpdateStripePaymentId(
                            id,
                            session.Id,
                            session.PaymentIntentId
                        );
                        orderHeaderRepository.UpdateStatus(
                            id,
                            SD.StatusApproved,
                            SD.PaymentStatusApproved
                        );
                        await orderHeaderRepository.Save();
                    }
                    HttpContext.Session.Clear();
                }
                var carts = await shoppingCartRepository.GetAll(s =>
                    s.ApplicationUserId == orderHeader.ApplicationUserId
                );
                shoppingCartRepository.RemoveRange(carts);
                await shoppingCartRepository.Save();
                return Ok(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        private double GetQuantatityPrice(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
                return shoppingCart.Product.Price;
            else
            {
                if (shoppingCart.Count <= 100)
                    return shoppingCart.Product.Price50;
                else
                    return shoppingCart.Product.Price100;
            }
        }
    }
}
