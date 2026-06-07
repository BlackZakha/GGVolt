using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GGVolt.Client.Models.Api;
using GGVolt.Client.Services;
using GGVolt.Client.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGVolt.Client.ViewModels;

public partial class AuthViewModel : ViewModelBase
{
    private readonly IAuthService _auth;

    // 🔑 Генератор MVVM Toolkit автоматически создаёт свойство + нотификации
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageTitle))]
    [NotifyPropertyChangedFor(nameof(SubmitText))]
    [NotifyPropertyChangedFor(nameof(ToggleText))]
    private bool _isRegistering;

    [ObservableProperty] private string _nickname = "";
    [ObservableProperty] private string _email = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _conpassword = "";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isVisibleField;
    [ObservableProperty] private string _errorMessage = "";

    // Вычисляемые свойства (обновляются автоматически благодаря NotifyPropertyChangedFor)
    public string PageTitle => IsRegistering ? "Создание аккаунта" : "Вход в аккаунт";
    public string SubmitText => IsRegistering ? "Зарегистрироваться" : "Войти";
    public string ToggleText => IsRegistering ? "Уже есть аккаунт? Войти" : "Нет аккаунта? Регистрация";
    public bool IsNotLoading => !IsLoading;

    public event EventHandler? AuthCompleted;

    public AuthViewModel(IAuthService auth) => _auth = auth;

    // 🔧 Этот метод вызывается автоматически при изменении _isRegistering
    partial void OnIsRegisteringChanged(bool value)
    {
        if (IsVisibleField) IsVisibleField = false;
        else IsVisibleField = true;
        Nickname = "";
        Email = "";
        Password = "";
        ErrorMessage = "";
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Nickname) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Заполните все поля";
            return;
        }

        IsLoading = true;
        ErrorMessage = "";

        try
        {
            if (IsRegistering)
            {
                if (Password == Conpassword)
                    await _auth.RegisterAsync(new RegisterRequest { Username = Nickname,  Email = Email, Password = Password},
                        CancellationToken.None);
                else throw(new Exception("Пароли не совпадают"));
            }
            else
                await _auth.LoginAsync(new LoginRequest { Username = Nickname, Password = Password }, CancellationToken.None);

            AuthCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            ErrorMessage = msg.Contains("401") || msg.Contains("400") ? "Неверный логин или пароль" : $"Ошибка: {msg}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand] private void ToggleMode() => IsRegistering = !IsRegistering;
    
    public void NotifyLogout()
    {
        IsRegistering = false;
        Nickname = Password = ErrorMessage = "";
    }
}