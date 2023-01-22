using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.WsPay.Models;
using System.Linq;

namespace Nop.Plugin.Payments.WsPay
{
    public class WsPayHelper
    {

        /// <summary>
        /// Get the generic attribute name that is used to store an order total that actually sent to PayPal (used to PDT order total validation)
        /// </summary>
        public static string OrderTotalSentToPayWay => "OrderTotalSentToWSPay";

        public static PaymentStatus GetPaymentStatus(string paymentStatus, string pendingReason)
        {
            var result = PaymentStatus.Pending;

            if (paymentStatus == null)
                paymentStatus = string.Empty;

            if (pendingReason == null)
                pendingReason = string.Empty;

            switch (paymentStatus.ToLowerInvariant())
            {
                case "pending":
                    switch (pendingReason.ToLowerInvariant())
                    {
                        case "authorization":
                            result = PaymentStatus.Authorized;
                            break;
                        default:
                            result = PaymentStatus.Pending;
                            break;
                    }
                    break;
                case "success":
                    result = PaymentStatus.Authorized;
                    break;
                case "failed":
                    result = PaymentStatus.Voided;
                    break;
                case "canceled":
                    result = PaymentStatus.Voided;
                    break;
                case "refunded":
                    result = PaymentStatus.Refunded;
                    break;
                default:
                    break;
            }
            return result;
        }

        public static PaymentStatus GetPaymentStatus(CallbackData callbackData)
        {
            var result = PaymentStatus.Pending;

            var callBackObjectType = callbackData.GetType();

            foreach (var prop in callBackObjectType.GetProperties().Where(a => a.PropertyType.Name == "string"))
            {
                if (prop.GetValue(callbackData).ToString() == "1")
                {
                    switch (prop.Name)
                    {
                        case nameof(PaymentStatus.Authorized):
                            result = PaymentStatus.Authorized;
                            break;
                        case nameof(PaymentStatus.Voided):
                            result = PaymentStatus.Voided;
                            break;
                        case nameof(PaymentStatus.Refunded):
                            result = PaymentStatus.Refunded;
                            break;
                        case "Completed":
                            result = PaymentStatus.Paid;
                            break;
                        default:
                            break;
                    }
                }
            }

            return result;
        }

        public static string GetReturnResultCodeText(string resultCode)
        {
            string result = string.Empty;

            switch (resultCode)
            {
                case "0":
                    result = "Akcija uspješna";
                    break;
                case "1":
                    result = "Akcija neuspješna";
                    break;
                case "2":
                    result = "Greška prilikom obrade";
                    break;
                case "3":
                    result = "Akcija otkazana";
                    break;
                case "4":
                    result = "Akcija neuspješna (3D Secure MPI)";
                    break;
                case "5":
                    result = "Autorizacija nije pronađena";
                    break;
                case "1000":
                    result = "Neispravan potpis(pgw_signature)";
                    break;
                case "1001":
                    result = "Neispravan ID dućana(pgw_shop_id)";
                    break;
                case "1002":
                    result = "Neispravan ID transakcija(pgw_transaction_id)";
                    break;
                case "1003":
                    result = "Neispravan iznos(pgw_amount) 1004 Neispravan tip autorizacije(pgw_authorization_type)";
                    break;
                case "1005":
                    result = "Neispravno trajanje najave autorizacije(pgw_announcement_duration)";
                    break;
                case "1006":
                    result = "Neispravan broj rata(pgw_installments)";
                    break;
                case "1007":
                    result = "Neispravan jezik(pgw_language)";
                    break;
                case "1008":
                    result = "Neispravan autorizacijski token(pgw_authorization_token)";
                    break;
                case "1100":
                    result = "Neispravan broj kartice(pgw_card_number)";
                    break;
                case "1101":
                    result = "Neispravan datum isteka kartice(pgw_card_expiration_date)";
                    break;
                case "1102":
                    result = "Neispravan verifikacijski broj kartice(pgw_card_verification_data)";
                    break;
                case "1200":
                    result = "Neispravan ID narudžbe(pgw_order_id)";
                    break;
                case "1201":
                    result = "Neispravan info narudžbe(pgw_order_info)";
                    break;
                case "1202":
                    result = "Neispravne stavke narudžbe(pgw_order_items)";
                    break;
                case "1300":
                    result = "Neispravan način povrata na dućan(pgw_return_method)";
                    break;
                case "1301":
                    result = "Neispravan povratni url na dućan(pgw_success_url)";
                    break;
                case "1302":
                    result = "Neispravan povratni url na dućan(pgw_failure_url)";
                    break;
                case "1304":
                    result = "Neispravni podaci trgovca(pgw_merchant_data)";
                    break;
                case "1400":
                    result = "Neispravno ime kupca(pgw_first_name)";
                    break;
                case "1401":
                    result = "Neispravno prezime kupca(pgw_last_name)";
                    break;
                case "1402":
                    result = "Neispravna adresa(pgw_street)";
                    break;
                case "1403":
                    result = "Neispravni grad(pgw_city)";
                    break;
                case "1404":
                    result = "Neispravni poštanski broj(pgw_post_code)";
                    break;
                case "1405":
                    result = "Neispravna država(pgw_country)";
                    break;
                case "1406":
                    result = "Neispravan kontakt telefon(pgw_telephone)";
                    break;
                case "1407":
                    result = "Neispravna kontakt e - mail adresa(pgw_email)";
                    break;
            }


            return result;
        }

        public static string GetCardTypeName(string wsPayCardTypeCode)
        {
            string result = wsPayCardTypeCode;

            switch (wsPayCardTypeCode)
            {
                case "1":
                    result = "Amex";
                    break;
                case "2":
                    result = "Diners";
                    break;
                case "3":
                    result = "Mastercard";
                    break;
                case "4":
                    result = "Visa";
                    break;
                default:
                    result = wsPayCardTypeCode;
                    break;
            }

            return result;
        }
    }
}
