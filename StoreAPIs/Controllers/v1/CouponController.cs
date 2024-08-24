using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Dto;
using StoreDataAccess.Repository.IRepository;
using StoreModels;
using StoreUtility;

namespace StoreAPIs.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class CouponController : ControllerBase
    {
        private readonly ILogger<CouponController> logger;
        private readonly ICouponRepository couponRepository;

        public CouponController(
            ILogger<CouponController> logger,
            ICouponRepository couponRepository
        )
        {
            this.logger = logger;
            this.couponRepository = couponRepository;
        }

        [HttpGet]
        public async Task<ActionResult<CouponDto>> GetAllCoupons()
        {
            try
            {
                var coupons = await couponRepository.GetAll();
                if (coupons is null)
                    return NotFound();
                var couponsDto = new List<CouponDto>();
                foreach (var item in coupons)
                {
                    couponsDto.Add(
                        new CouponDto
                        {
                            CouponId = item.CouponId,
                            CouponCode = item.CouponCode,
                            MinAmount = item.MinAmount,
                            DiscountAmount = item.DiscountAmount
                        }
                    );
                }

                return Ok(couponsDto);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<ActionResult<CouponDto>> Get(int id)
        {
            try
            {
                var coupon = await couponRepository.Get(x => x.CouponId == id);
                if (coupon is null)
                    return NotFound();
                var couponDto = new CouponDto
                {
                    CouponId = coupon.CouponId,
                    CouponCode = coupon.CouponCode,
                    MinAmount = coupon.MinAmount,
                    DiscountAmount = coupon.DiscountAmount
                };
                return Ok(couponDto);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet]
        [Route("GetByCode/{code}")]
        public async Task<ActionResult<CouponDto>> GetByCode(string code)
        {
            try
            {
                var coupon = await couponRepository.Get(x => x.CouponCode == code);
                if (coupon is null)
                    return NotFound();
                var couponDto = new CouponDto
                {
                    CouponId = coupon.CouponId,
                    CouponCode = coupon.CouponCode,
                    MinAmount = coupon.MinAmount,
                    DiscountAmount = coupon.DiscountAmount
                };
                return Ok(couponDto);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<CouponDto>> Post([FromBody] CouponDto couponDto)
        {
            try
            {
                Coupon coupon = new Coupon
                {
                    CouponId = couponDto.CouponId,
                    CouponCode = couponDto.CouponCode,
                    MinAmount = couponDto.MinAmount,
                    DiscountAmount = couponDto.DiscountAmount
                };

                await couponRepository.Add(coupon);
                await couponRepository.Save();

                var options = new Stripe.CouponCreateOptions
                {
                    AmountOff = (long)(couponDto.DiscountAmount * 100),
                    Name = couponDto.CouponCode,
                    Currency = "usd",
                    Id = couponDto.CouponCode,
                };
                var service = new Stripe.CouponService();
                service.Create(options);
                couponDto = new CouponDto
                {
                    CouponId = coupon.CouponId,
                    CouponCode = coupon.CouponCode,
                    MinAmount = coupon.MinAmount,
                    DiscountAmount = coupon.DiscountAmount
                };
                return Ok(couponDto);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<CouponDto>> Put([FromBody] CouponDto couponDto)
        {
            try
            {
                Coupon coupon = new Coupon
                {
                    CouponId = couponDto.CouponId,
                    CouponCode = couponDto.CouponCode,
                    MinAmount = couponDto.MinAmount,
                    DiscountAmount = couponDto.DiscountAmount
                };
                couponRepository.Update(coupon);
                await couponRepository.Save();

                couponDto = new CouponDto
                {
                    CouponId = coupon.CouponId,
                    CouponCode = coupon.CouponCode,
                    MinAmount = coupon.MinAmount,
                    DiscountAmount = coupon.DiscountAmount
                };
                return Ok(couponDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Coupon coupon = await couponRepository.Get(u => u.CouponId == id);
                couponRepository.Remove(coupon);
                await couponRepository.Save();

                var service = new Stripe.CouponService();
                service.Delete(coupon.CouponCode);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
