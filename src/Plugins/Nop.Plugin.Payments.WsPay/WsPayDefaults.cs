namespace Nop.Plugin.Payments.WsPay
{
    public static class WsPayDefaults
    {
        public const string ERROR_TOPIC_PAGE_SYSTEM_NAME = "wspayerror";
        public const string ERROR_TOPIC_PAGE_TITLE = "WsPay error";
        public const string ERROR_TOPIC_PAGE_BODY = "<h3><span style=\"color: #000000;\">Došlo je do pogreške prilikom autorizacije vaše kartice.</span><br /><span style=\"color: #000000;\">Vašu narudžbu možete pronaći u vašem profilu pod linkom <a href=\"/order/history\">NARUDŽBE </a>i pokušati ponovo ili nas kontaktirajte</span>:</h3><p> </p><h2>Web Shop</h2><p><strong>Upiši ime shopa</strong></p><p>Ulica 00, 00000 Grad, Država</p><p>Tel: 00 000 000<br />Mobile: 000 0000 000<br />E-mail: email @email.hr</p>";
        public const string ERROR_TOPIC_PAGE_SE_NAME = "wspay-error";
        public const string RATE_PAYMENT_CARDS_FILE_PATH = "Plugins/Payments.WsPay/ratePaymentCardsDefaults.json";
    }
}
