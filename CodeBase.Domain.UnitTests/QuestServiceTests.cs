using CodeBase.EFCore.Data.Model;
using CodeBase.EFCore.Data.Repository.Interface;
using Moq;
using System.Text.Json;
using CodeBase.Service;

namespace CodeBase.Domain.UnitTests;

public class QuestServiceTests
{
    private readonly Mock<IQuestRepository> _questRepositoryMock;
    private readonly QuestService _questService;

    public QuestServiceTests()
    {
        _questRepositoryMock = new Mock<IQuestRepository>();
        _questService = new QuestService(_questRepositoryMock.Object);
    }

    [Fact]
    public async Task InitializeQuestsAsync_ReturnsTrue_WhenQuestsAreLoaded()
    {
        // Arrange
        var quests = new List<Quest> { new Quest { Id = 1, Name = "Quest1" } };
        File.WriteAllText("quests.json", JsonSerializer.Serialize(quests));
        _questRepositoryMock.Setup(repo => repo.InitializeQuests(quests))
            .ReturnsAsync(false);
        // Act
        var result = await _questService.InitializeQuestsAsync();
        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task InitializeQuestsAsync_ReturnsFalse_WhenNoQuestsAreLoaded()
    {
        // Arrange
        File.WriteAllText("quests.json", "[]");
        // Act
        var result = await _questService.InitializeQuestsAsync();
        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllQuestsAsync_ReturnsListOfQuests()
    {
        // Arrange
        var quests = new List<Quest> { new Quest { Id = 1, Name = "Quest1" } };
        _questRepositoryMock.Setup(repo => repo.GetAllQuestsAsync())
            .ReturnsAsync(quests);
        //Act
        var result = await _questService.GetAllQuestsAsync();
        //Assert
        Assert.Equal(quests, result);
    }
}