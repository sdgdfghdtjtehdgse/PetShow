using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShow.Models;
using System.ComponentModel.DataAnnotations;

namespace PetShow.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Введите полное имя")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(8, ErrorMessage = "Пароль должен содержать не менее 8 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }
    public List<string> Errors { get; set; } = new();

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        if (!ModelState.IsValid)
        {
            Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Page();
        }

        // Check passwords match explicitly
        if (Input.Password != Input.ConfirmPassword)
        {
            Errors.Add("Пароли не совпадают.");
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName    = Input.Email,
            Email       = Input.Email,
            FullName    = Input.FullName,
            CreatedAt   = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            // Обычная роль пользователя
            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(ReturnUrl);
        }

        // Ошибка авторизации
        foreach (var error in result.Errors)
        {
            Errors.Add(TranslateError(error.Code, error.Description));
        }

        return Page();
    }

    private static string TranslateError(string code, string fallback) => code switch
    {
        "DuplicateUserName"    => "Пользователь с таким email уже существует.",
        "DuplicateEmail"       => "Этот email уже зарегистрирован.",
        "InvalidEmail"         => "Некорректный адрес email.",
        "PasswordTooShort"     => "Пароль слишком короткий. Минимум 8 символов.",
        "PasswordRequiresDigit"=> "Пароль должен содержать хотя бы одну цифру.",
        "PasswordRequiresUpper"=> "Пароль должен содержать хотя бы одну заглавную букву.",
        "PasswordRequiresLower"=> "Пароль должен содержать хотя бы одну строчную букву.",
        "PasswordRequiresNonAlphanumeric" => "Пароль должен содержать хотя бы один специальный символ.",
        _ => fallback
    };
}