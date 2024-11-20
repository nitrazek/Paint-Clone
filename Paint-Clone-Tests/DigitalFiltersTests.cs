using Paint_Clone.DigitalFiltersMode.Viewmodels;

namespace Paint_Clone_Tests;

[TestClass]
public class DigitalFiltersTests
{
    private DigitalFiltersViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _viewModel = new DigitalFiltersViewModel();
    }

    private byte[] CreateTestImage(int width, int height, byte color)
    {
        int stride = width * 4; // 4 bajty na piksel (ARGB)
        byte[] image = new byte[height * stride];
        for (int i = 0; i < image.Length; i += 4)
        {
            image[i] = color;       // Blue
            image[i + 1] = color;   // Green
            image[i + 2] = color;   // Red
            image[i + 3] = 255;     // Alpha
        }
        return image;
    }

    [TestMethod]
    public void ApplySmoothingFilter_ShouldSmoothImage()
    {
        // Arrange
        int width = 5, height = 5;
        byte initialColor = 100;
        var image = CreateTestImage(width, height, initialColor);

        // Act
        _viewModel.ApplySmoothingFilter(image, width, height, width * 4);

        // Assert
        Assert.IsTrue(image.All(b => b >= initialColor / 2 && b <= initialColor), "Pixels were not smoothed correctly.");
    }

    [TestMethod]
    public void ApplyMedianFilter_ShouldReduceNoise()
    {
        // Arrange
        int width = 3, height = 3;
        byte[] noisyImage = {
                0, 0, 0, 255,  255, 255, 255, 255,  0, 0, 0, 255,
                255, 255, 255, 255,  128, 128, 128, 255,  255, 255, 255, 255,
                0, 0, 0, 255,  255, 255, 255, 255,  0, 0, 0, 255,
            };

        // Act
        _viewModel.ApplyMedianFilter(noisyImage, width, height, width * 4);

        // Assert
        byte medianPixel = noisyImage[(width * height / 2) * 4]; // Œrodkowy piksel
        Assert.AreEqual(128, medianPixel, "Median filter did not compute the correct median value.");
    }

    [TestMethod]
    public void ApplySobelFilter_ShouldDetectEdges()
    {
        // Arrange
        int width = 3, height = 3;
        byte[] edgeImage = {
                0, 0, 0, 255,  0, 0, 0, 255,  0, 0, 0, 255,
                0, 0, 0, 255,  255, 255, 255, 255,  0, 0, 0, 255,
                0, 0, 0, 255,  0, 0, 0, 255,  0, 0, 0, 255,
            };

        // Act
        _viewModel.ApplySobelFilter(edgeImage, width, height, width * 4);

        // Assert
        byte centerPixel = edgeImage[(width * height / 2) * 4]; // Œrodkowy piksel
        Assert.IsTrue(centerPixel > 0, "Sobel filter did not detect edges correctly.");
    }
}