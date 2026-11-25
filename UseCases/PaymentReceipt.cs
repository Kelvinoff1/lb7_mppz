using StoreSystem.Models;

namespace StoreSystem.UseCases;

public record PaymentReceipt(Payment Payment, decimal Subtotal, decimal Discount);
