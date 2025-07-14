using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CNPM_Project_web.Lib
{
    public static class VNPayConfig
    {
        public static string vnp_TmnCode = "UEOBZMHW"; // mã test
        public static string vnp_HashSecret = "OARYW60U7W9SA5RAXNXZGUTYGEWYZAQZ"; // key test
        public static string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        public static string vnp_ReturnUrl = "https://localhost:44300/Order/VNPayReturn"; // URL xử lý sau khi thanh toán
    }
}
