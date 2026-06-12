using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GGVolt.Client.Models.Api;
using GGVolt.Client.Services;

namespace GGVolt.Client.ViewModels;

public partial class AuthViewModel : ViewModelBase
{
    private readonly IAuthService _auth;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PageTitle))]
    [NotifyPropertyChangedFor(nameof(SubmitText))] [NotifyPropertyChangedFor(nameof(ToggleText))]
    private bool _isRegistering;

    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _email = ""; // ✅ Новое поле
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = "";

    public string PageTitle => IsRegistering ? "Создание аккаунта" : "Вход в аккаунт";
    public string SubmitText => IsRegistering ? "Зарегистрироваться" : "Войти";
    public string ToggleText => IsRegistering ? "Уже есть аккаунт? Войти" : "Нет аккаунта? Регистрация";
    public bool IsNotLoading => !IsLoading;

    public event EventHandler? AuthCompleted;

    public AuthViewModel(IAuthService auth) => _auth = auth;

    partial void OnIsRegisteringChanged(bool value)
    {
        Username = Email = Password = ErrorMessage = "";
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Заполните все поля";
            return;
        }

        if (IsRegistering && string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Укажите email";
            return;
        }

        IsLoading = true;
        ErrorMessage = "";

        try
        {
            if (IsRegistering)
            {
                await _auth.RegisterAsync(new RegisterRequest(Username.Trim(), Email.Trim().ToLowerInvariant(), Password), CancellationToken.None);
            }
            else
            {
                await _auth.LoginAsync(new LoginRequest(Username, Password), CancellationToken.None);
            }

            AuthCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            ErrorMessage = msg.Contains("401") || msg.Contains("400") || msg.Contains("409")
                ? "Неверный логин, пароль или email уже занят"
                : $"Ошибка: {msg}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand] private void ToggleMode() => IsRegistering = !IsRegistering;
    public void NotifyLogout() { IsRegistering = false; Username = Email = Password = ErrorMessage = ""; }
}