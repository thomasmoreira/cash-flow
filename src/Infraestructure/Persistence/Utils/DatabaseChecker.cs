using System.Data.Common;

namespace CashFlow.Infraestructure.Persistence.Utils;

public static class DatabaseChecker
{
    public async static Task WaitForDatabaseAsync(ApplicationDbContext context, int retryCount = 10, int delaySeconds = 5)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                if (await context.Database.CanConnectAsync())
                {
                    return;
                }
            }
            catch (DbException)
            {
                // Ignora e tenta novamente
            }
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
        throw new Exception("Não foi possível conectar ao banco de dados após várias tentativas.");
    }
}
