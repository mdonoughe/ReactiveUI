using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using ReactiveUI;
using ReactiveUI.Testing;
using Shouldly;
using Xunit;

namespace IntegrationTests.Shared.Tests.Features.Login
{
    public class LoginViewModelTests
    {
        [Fact]
        public void LoginButton_IsDisabled_ByDefault()
        {
            var sut = new LoginViewModelBuilder()
                .Build();

            sut.Login.CanExecute
                .FirstAsync().Wait()
                .ShouldBe(false);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(null, "")]
        [InlineData(" ", "")]
        [InlineData("", " ")]
        [InlineData(" ", " ")]
        public async Task LoginButton_IsDisabled_WhenUserNameOrPassword_IsEmpty(string userName, string password)
        {
            var sut = new LoginViewModelBuilder()
                .WithUserName(userName)
                .WithPassword(password)
                .Build();

            (await sut.Login.CanExecute.FirstAsync()).ShouldBe(false);
        }

        [Theory]
        [InlineData("coolusername", "excellentpassword")]
        public async Task LoginButton_IsEnabled_WhenUserNameAndPassword_IsNotEmptyAsync(string userName, string password)
        {
            var sut = new LoginViewModelBuilder()
                .WithUserName(userName)
                .WithPassword(password)
                .Build();

            (await sut.Login.CanExecute.FirstAsync()).ShouldBe(true);
        }

        [Fact]
        public async Task CancelButton_IsDisabled_WhenNot_LoggingIn()
        {
            var sut = new LoginViewModelBuilder()
                .Build();

            (await sut.Cancel.CanExecute.FirstAsync()).ShouldBe(false);
        }

        [Fact]
        public void CancelButton_Cancels_Login()
        {
            var scheduler = new TestScheduler();

            var sut = new LoginViewModelBuilder()
                .WithScheduler(scheduler)
                    .WithUserName("coolusername")
                    .WithPassword("excellentpassword")
                    .Build();

            scheduler.AdvanceByMs(TimeSpan.FromSeconds(1).Milliseconds);

            sut.Login.Subscribe(x => x.ShouldBe(true));

            Observable
                .Return(Unit.Default)
                .InvokeCommand(sut.Login);

            sut.Cancel.CanExecute.Subscribe(x => x.ShouldBe(true));

            scheduler.AdvanceByMs(TimeSpan.FromSeconds(1).Milliseconds);

            Observable
                .Return(Unit.Default)
                .InvokeCommand(sut.Cancel);
        }

        [Fact]
        public void CancelButton_IsAvailableUntil_TwoSeconds()
        {
            bool actual = false;
            var scheduler = new TestScheduler();

            var sut = new LoginViewModelBuilder()
                .WithScheduler(scheduler)
                .WithUserName("coolusername")
                .WithPassword("excellentpassword")
                .Build();


            sut.Cancel.CanExecute.Subscribe(x => 
            {
                actual = x;   
            });

            sut.Login.IsExecuting.Subscribe(x => {
                var a = x;
            });

            sut.Login.Subscribe();
            Observable.Return(Unit.Default).InvokeCommand(sut.Login);

            actual.ShouldBe(false);

            // 50ms
            scheduler.AdvanceByMs(50);

            actual.ShouldBe(true);

            // 1sec 50ms
            scheduler.AdvanceByMs(TimeSpan.FromSeconds(1).Milliseconds);

            actual.ShouldBe(true);

            // 2sec 50sms
            scheduler.AdvanceByMs(TimeSpan.FromSeconds(1).Milliseconds);

            actual.ShouldBe(false);
        }

        [Fact]
        public void User_CannotLogin_WithIncorrect_Password()
        {
            var sut = new LoginViewModelBuilder()
                .WithUserName("coolusername")
                .WithPassword("incorrectpassword")
                .Build();

            sut.Login.Subscribe(x => x.ShouldBe(true));

            Observable.Return(Unit.Default).InvokeCommand(sut.Login);
        }

        [Fact]
        public void User_CanLogin_WithCorrect_Password()
        {
            var sut = new LoginViewModelBuilder()
                .WithUserName("coolusername")
                .WithPassword("Mr. Goodbytes")
                .Build();

            sut.Login.Subscribe(x => x.ShouldBe(true));

            Observable.Return(Unit.Default).InvokeCommand(sut.Login);
        }

    }
}
