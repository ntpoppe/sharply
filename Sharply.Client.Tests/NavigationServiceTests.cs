using Moq;
using NUnit.Framework;
using Sharply.Client.Interfaces;
using Sharply.Client.Services;

namespace Sharply.Client.Tests;

[TestFixture]
public class NavigationServiceTests
{
    private Mock<IServiceProvider>? _serviceProviderMock;
    private NavigationService? _navigationService;

    public class MockNonNavigableViewModel { }

    public class MockNavigableViewModel : INavigable
    {
        public object? NavigatedParameter { get; private set; }

        public void OnNavigatedTo(object? parameter)
        {
            NavigatedParameter = parameter;
        }
    }

    [SetUp]
    public void Setup()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _navigationService = new NavigationService(_serviceProviderMock.Object);
    }

    [Test]
    public void NavigateTo_ValidNavigableViewModel_SetsCurrentView()
    {
        if (_serviceProviderMock == null || _navigationService == null)
            throw new Exception("Services were null");

        // Arrange
        var navigableViewModel = new Mock<INavigable>();
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(MockNavigableViewModel)))
                            .Returns(navigableViewModel.Object);

        // Act
        var result = _navigationService.NavigateTo<MockNavigableViewModel>("TestParameter");

        // Assert
        Assert.That(result, Is.EqualTo(navigableViewModel.Object));
        Assert.That(_navigationService.CurrentView, Is.EqualTo(navigableViewModel.Object));
        navigableViewModel.Verify(vm => vm.OnNavigatedTo("TestParameter"), Times.Once);
    }

    [Test]
    public void NavigateTo_NonNavigableViewModel_ThrowsException()
    {
        if (_serviceProviderMock == null || _navigationService == null)
            throw new Exception("Services were null");

        // Arrange
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(MockNonNavigableViewModel)))
                            .Returns(new MockNonNavigableViewModel());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            _navigationService.NavigateTo<MockNonNavigableViewModel>();
        });
    }


    [Test]
    public void GoBack_PopsStackAndUpdatesCurrentView()
    {
        if (_serviceProviderMock == null || _navigationService == null)
            throw new Exception("Services were null");

        // Arrange
        var viewModel1 = new MockNavigableViewModel();
        var viewModel2 = new MockNavigableViewModel();
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(MockNavigableViewModel)))
                            .Returns(viewModel1);
        _navigationService.NavigateTo<MockNavigableViewModel>();
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(MockNavigableViewModel)))
                            .Returns(viewModel2);
        _navigationService.NavigateTo<MockNavigableViewModel>();

        // Act
        _navigationService.GoBack();

        // Assert
        Assert.That(_navigationService.CurrentView, Is.EqualTo(viewModel1));
    }

    [Test]
    public void GoBack_DoesNothingIfStackHasOneItem()
    {
        if (_serviceProviderMock == null || _navigationService == null)
            throw new Exception("Services were null");

        // Arrange
        var viewModel = new MockNavigableViewModel();
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(MockNavigableViewModel)))
                            .Returns(viewModel);
        _navigationService.NavigateTo<MockNavigableViewModel>();

        // Act
        _navigationService.GoBack();

        // Assert
        Assert.That(_navigationService.CurrentView, Is.EqualTo(viewModel));
    }

    [Test]
    public void NavigateTo_RaisesPropertyChangedForCurrentView()
    {
        if (_serviceProviderMock == null || _navigationService == null)
            throw new Exception("Services were null");

        // Arrange
        var viewModel = new MockNavigableViewModel();
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(MockNavigableViewModel)))
                            .Returns(viewModel);
        bool eventRaised = false;
        _navigationService.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(_navigationService.CurrentView))
            {
                eventRaised = true;
            }
        };

        // Act
        _navigationService.NavigateTo<MockNavigableViewModel>();

        // Assert
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void NavigateTo_UnregisteredViewModel_ThrowsException()
    {
        if (_navigationService == null)
            throw new Exception("Services were null");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            _navigationService.NavigateTo<MockNavigableViewModel>();
        });
    }
}

