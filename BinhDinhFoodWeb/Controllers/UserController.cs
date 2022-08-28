﻿using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using BinhDinhFoodWeb.Intefaces;
using System.Security.Claims;
using BinhDinhFoodWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace BinhDinhFoodWeb.Controllers
{
    [AllowAnonymous]
    public class UserController : Controller
    {
        private readonly IUserRepository _repo;
        private readonly IUserManager _userManager;
        private readonly IMailService _mailService;
        private readonly ITokenRepository _tokenRepository;
        public UserController(IUserRepository repo, IUserManager userManager, IMailService mailService, ITokenRepository tokenRepository)
        {
            _repo = repo;
            _userManager = userManager;
            _mailService = mailService;
            _tokenRepository = tokenRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _repo.Register(model);

            // +1 line added for SignIn
            await _userManager.SignIn(this.HttpContext, user, false);

            return LocalRedirect("~/Home/Index");
        }
        [HttpGet]
        public IActionResult Login()
        {
           return View();
        }
        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginViewModel model)
        {
            if (!ModelState.IsValid)
               // return View(model);
                return RedirectToAction("Index", "User");
            var user = _repo.Validate(model);

            if (user == null) 
                return View(model);
                //return RedirectToAction("ShowMessage");

            // +1 line added for SignIn
            await _userManager.SignIn(this.HttpContext, user, model.RememberLogin);

            //return LocalRedirect("~/Home/Index");
            return RedirectToAction("Index","Home");
        }
        public async Task<IActionResult> Logout(string returnUrl)
        {
            await _userManager.SignOut(this.HttpContext);

            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPasswordAsync(ForgotViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _repo.HaveAccount(model);
                if (!user)
                {
                    ViewBag.Message = "Your username or email is wrong!";
                    return View(model);
                }
                string linkResetPassword = _repo.CreateResetPasswordLink(model.CustomerUserName);
                await _mailService.SendEmailAsync(new MailRequest(model.CustomerEmail,"Reset password",linkResetPassword));
                return RedirectToAction("ShowMessage");
            }
            else
            {
                ViewBag.Message = "Please fill out all information before submitting";
                return View(model);
            }
        }
        [HttpGet]
        public IActionResult ShowMessage()
        {
            ViewBag.Message = "Password reset link has been sent to your email";
            return View();
        }
        [HttpGet]
        public IActionResult ResetPassword(string user,string token)
        {
            ViewBag.Message = null;
            bool checkedToken = _tokenRepository.CheckToken(user, token);
            if (checkedToken)
            {
                ViewBag.CustomerUserName = user;
                ViewBag.Token = token;
                return View();
            }
            return RedirectToAction("Login");
        }
        [HttpPost]
        public async Task<IActionResult> ResetPasswordAsync(ResetViewModel model)
        {
            ViewBag.Message = "hi";
            if (ModelState.IsValid)
            {
                bool checkedToken = _tokenRepository.CheckToken(model.CustomerUserName,model.Token);
                if (checkedToken)
                {
                    await _repo.ResetPassWord(model);
                    return RedirectToAction("Index","Home");
                }
                return RedirectToAction("Login");
            }
                ViewBag.CustomerUserName = model.CustomerUserName;
                ViewBag.Token = model.Token;
                return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult ChangeInfor()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login");
            int id = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //ViewBag.ID = id;
            ChangeInforViewModel model = _repo.GetUserInfor(id); 
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeInforAsync(ChangeInforViewModel model)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login");
            int id = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _repo.ChangeInforUser(model, id);
            return View(model);
        }
        [HttpGet]
        public IActionResult ChangePass()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassAsync(ChangePasswordViewModel model)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login");
            if (!ModelState.IsValid)return View(model);
            int id = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string user = User.FindFirstValue(ClaimTypes.Name);
            if (await _repo.HaveAccount(user, model.OldPassword))
            {
                await _repo.ChangePasswordUser(model, id);
                return View();
            }
            ViewBag.Message = "Your password is wrong :(((";
            return View();
        }
    }
}