using Xunit;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;

namespace EscolaBiblica.Tests.Integration;

[ExcludeFromCodeCoverage] // Testes de integração não contribuem para cobertura de código
public class BasicIntegrationTests
{
    [Fact]
    public void Integration_ShouldPassBasicTest()
    {
        // Arrange
        var expectedValue = 42;
        
        // Act
        var actualValue = 42;
        
        // Assert
        actualValue.Should().Be(expectedValue);
    }

    [Fact]
    public void Integration_ShouldValidateDateTime()
    {
        // Arrange
        var now = DateTime.Now;
        
        // Act
        var isValid = now.Year >= 2000;
        
        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Integration_ShouldValidateGuidGeneration()
    {
        // Arrange & Act
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        
        // Assert
        guid1.Should().NotBe(Guid.Empty);
        guid2.Should().NotBe(Guid.Empty);
        guid1.Should().NotBe(guid2);
    }
}