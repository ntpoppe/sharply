using Moq;
using NUnit.Framework;
using Sharply.Client.Services;
using Sharply.Client.Interfaces;

namespace Sharply.Client.Tests;

[TestFixture]
public class OverlayServiceTests
{
	private Mock<IServiceProvider>? _serviceProviderMock;
	private OverlayService? _overlayService;

	public class MockOverlayViewModel : IOverlay
	{
		public void Close() { }
	}

	public class MockNonOverlayViewModel { }

	[SetUp]
	public void Setup()
	{
		_serviceProviderMock = new Mock<IServiceProvider>();
		_overlayService = new OverlayService(_serviceProviderMock.Object);
	}

	[Test]
	public void ShowOverlay_ValidOverlay_SetsCurrentOverlayViewAndVisibility()
	{
		if (_serviceProviderMock == null || _overlayService== null)
			throw new Exception("Services were null");

		// Arrange
		var overlayView = new Mock<IOverlay>();
		_serviceProviderMock.Setup(sp => sp.GetService(typeof(MockOverlayViewModel)))
							.Returns(overlayView.Object);

		// Act
		_overlayService.ShowOverlay<MockOverlayViewModel>();

		// Assert
		Assert.That(_overlayService.CurrentOverlayView, Is.EqualTo(overlayView.Object));
		Assert.That(_overlayService.IsOverlayVisible, Is.True);
	}

	[Test]
	public void ShowOverlay_UnregisteredOverlay_ThrowsException()
	{
		if (_overlayService== null)
			throw new Exception("Services were null");

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() =>
		{
			_overlayService.ShowOverlay<MockOverlayViewModel>();
		});
	}

	[Test]
	public void HideOverlay_ResetsCurrentOverlayViewAndVisibility()
	{
		if (_serviceProviderMock == null || _overlayService== null)
			throw new Exception("Services were null");

		// Arrange
		var overlayView = new Mock<IOverlay>();
		_serviceProviderMock.Setup(sp => sp.GetService(typeof(MockOverlayViewModel)))
							.Returns(overlayView.Object);
		_overlayService.ShowOverlay<MockOverlayViewModel>();

		// Act
		_overlayService.HideOverlay();

		// Assert
		Assert.That(_overlayService.CurrentOverlayView, Is.Null);
		Assert.That(_overlayService.IsOverlayVisible, Is.False);
	}

	[Test]
	public void ShowOverlay_RaisesPropertyChangedForIsOverlayVisible()
	{
		if (_serviceProviderMock == null || _overlayService== null)
			throw new Exception("Services were null");

		// Arrange
		var overlayView = new Mock<IOverlay>();
		_serviceProviderMock.Setup(sp => sp.GetService(typeof(MockOverlayViewModel)))
							.Returns(overlayView.Object);
		bool eventRaised = false;
		_overlayService.PropertyChanged += (sender, args) =>
		{
			if (args.PropertyName == nameof(_overlayService.IsOverlayVisible))
			{
				eventRaised = true;
			}
		};

		// Act
		_overlayService.ShowOverlay<MockOverlayViewModel>();

		// Assert
		Assert.That(eventRaised, Is.True);
	}
}

