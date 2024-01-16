using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp_Core_Identity.Model;
using WebApp_Core_Identity.ViewModels;

namespace WebApp_Core_Identity.Pages
{
    //Initialize the build-in ASP.NET Identity
    public class RegisterModel : PageModel
    {
        private UserManager<ApplicationUser> userManager { get; }
        private SignInManager<ApplicationUser> signInManager { get; }

        private readonly RoleManager<IdentityRole> roleManager;

        [BindProperty]
        public Register RModel { get; set; }
        public RegisterModel(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole>roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
			this.roleManager = roleManager;
        }

		public async Task<IActionResult> OnPostAsync()
		{
			if (ModelState.IsValid)
			{
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");

				var user = new ApplicationUser()
				{
					UserName = RModel.Email,
					Email = RModel.Email,
                    FullName = RModel.FullName,
                    CreditCard = protector.Protect(RModel.CreditCard)
				};

                //Create the Admin role if NOT exist
                IdentityRole role = await roleManager.FindByIdAsync("Admin");
                if (role == null)
                {
                    IdentityResult result2 = await roleManager.CreateAsync(new IdentityRole("Admin"));
                    if (!result2.Succeeded)
                    {
                        ModelState.AddModelError("", "Create role admin failed");
                    }
                }

                //Create the HR role if NOT exist
                IdentityRole hrrole = await roleManager.FindByIdAsync("HR");
                if (hrrole == null)
                {
                    IdentityResult result2 = await roleManager.CreateAsync(new IdentityRole("HR"));
                    if (!result2.Succeeded)
                    {
                        ModelState.AddModelError("", "Create role hr failed");
                    }
                }

                var result = await userManager.CreateAsync(user, RModel.Password);

				if (result.Succeeded)
				{
                    //Add users to Admin Role
                    result = await userManager.AddToRoleAsync(user, "Admin");

                    //Add users to HR Role
                    result = await userManager.AddToRoleAsync(user, "HR");

                    await signInManager.SignInAsync(user, false);
					return RedirectToPage("Index");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}

			return Page();
		}
			

			public void OnGet()
        {
            
            
            
        }
    }
}
