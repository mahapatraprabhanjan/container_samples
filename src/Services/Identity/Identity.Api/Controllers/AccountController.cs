using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.Api.Models;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly IClientStore _clientStore;
        private readonly IIdentityServerInteractionService _identityServerInteractionService;

        public AccountController(IClientStore clientStore, IIdentityServerInteractionService identityServerInteractionService)
        {
            _clientStore = clientStore ?? throw new ArgumentNullException(nameof(clientStore));
            _identityServerInteractionService = identityServerInteractionService ?? throw new ArgumentNullException(nameof(identityServerInteractionService));
        }

        public async Task<IActionResult> Login(string returnUrl)
        {
            var context = await _identityServerInteractionService.GetAuthorizationContextAsync(returnUrl);

            if(context?.IdP != null)
            {
                //Return External Login
            }

            var viewModel = await BuildLoginViewModel(returnUrl, context);

            ViewBag["ReturnUrl"] = returnUrl;

            return View();
        }

        private async Task<LoginViewModel> BuildLoginViewModel(string returnUrl, AuthorizationRequest context)
        {
            var allowedLocal = true;
            if(context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if(client != null)
                {
                    allowedLocal = client.EnableLocalLogin;
                }
            }

            return new LoginViewModel
            {
                ReturnUrl = returnUrl,
                Email = context?.LoginHint
            };
        }
    }
}