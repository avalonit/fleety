namespace AITrackerAgent.Interfaces;

public interface IAgentService
{
    Task CreateChatBot();
    Task AddMemory(string question, string questionId);
    void AddLocalMemory(string question);
    Task<string> AddMessage(string question);
}

