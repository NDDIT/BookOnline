﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NguyenDuyDuong.SachOnline.Views
{
    public class SachOnlineController : Controller
    {
        // GET: SachOnline
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult NavPartial()
        {
            return PartialView();
        }
    }
}