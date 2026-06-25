namespace MyWeb.Application.DTOs.Storefront.Cart;

public sealed class CartCouponPreviewResult
{
    private CartCouponPreviewResult(
        bool succeeded,
        string message,
        string couponCode,
        string couponName,
        string subtotalText,
        string discountTotalText,
        string estimatedTotalText,
        bool canCheckout)
    {
        Succeeded = succeeded;
        Message = message;
        CouponCode = couponCode;
        CouponName = couponName;
        SubtotalText = subtotalText;
        DiscountTotalText = discountTotalText;
        EstimatedTotalText = estimatedTotalText;
        CanCheckout = canCheckout;
    }

    public bool Succeeded { get; }

    public string Message { get; }

    public string CouponCode { get; }

    public string CouponName { get; }

    public string SubtotalText { get; }

    public string DiscountTotalText { get; }

    public string EstimatedTotalText { get; }

    public bool CanCheckout { get; }

    public static CartCouponPreviewResult Create(
        bool succeeded,
        string message,
        string couponCode,
        string couponName,
        string subtotalText,
        string discountTotalText,
        string estimatedTotalText,
        bool canCheckout)
    {
        return new CartCouponPreviewResult(
            succeeded,
            message,
            couponCode,
            couponName,
            subtotalText,
            discountTotalText,
            estimatedTotalText,
            canCheckout);
    }
}
