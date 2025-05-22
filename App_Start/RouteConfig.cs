using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NguyenDuyDuong.SachOnline
{
    public class RouteConfig
    {

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               name: "Trang chu",
               url: "",
               defaults: new { controller = "BookStore", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Sach theo Chu de",
                url: "sach-theo-chu-de/{id}/{page}",
                defaults: new { controller = "BookStore", action = "SPTheoChuDe", page = UrlParameter.Optional },
                namespaces: new string[] { "BookStore.Controllers" }
            );

            routes.MapRoute(
                name: "Sach theo NXB",
                url: "sach-theo-nxb/{id}/{page}",
                defaults: new { controller = "BookStore", action = "SPTheoNhaXuatBan", page = UrlParameter.Optional },
                namespaces: new string[] { "BookStore.Controllers" }
            );


            routes.MapRoute(
                name: "Chi tiet sach",
                url: "chi-tiet-sach/{id}",
                defaults: new { controller = "BookStore", action = "Details", id = UrlParameter.Optional },
                namespaces: new string[] { "BookStore.Controllers" }
            );


            routes.MapRoute(
               name: "Dang ky",
               url: "dang-ky",
               defaults: new { controller = "User", action = "DangKy" },
               namespaces: new string[] { "User.Controllers" }
            );

            routes.MapRoute(
               name: "Dang nhap",
               url: "dang-nhap",
               defaults: new { controller = "User", action = "DangNhap", url = UrlParameter.Optional },
               namespaces: new string[] { "User.Controllers" }
            );

            routes.MapRoute(
                name: "Trang tin",
                url: "{metatitle}",
                defaults: new { controller = "BookStore", action = "TrangTin", metatitle = UrlParameter.Optional }
               
            );
            routes.MapRoute(
               name: "Default",
               url: "{controller}/{action}/{id}",
               defaults: new { controller = "BookStore", action = "Index", id = UrlParameter.Optional }
           );
        }
    }
}
