#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0050

using PERI.SK.Domain.Models;
using PERI.SK.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;

namespace PERI.SK.Infrastructure
{
    public partial class ChatService
    {
        /// <summary>
        /// Processes PDF ChatCompletion
        /// </summary>
        /// <param name="refData"></param>
        /// <param name="chatHistory"></param>
        /// <param name="connectionString"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task<string> ProcessPdf(ReferenceData refData, ChatHistory chatHistory, string connectionString, string? query = null)
        {
            if (!_kernel.Plugins.Contains(refData.Subject))
            {
                var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
                var memory = new SemanticTextMemory(new VolatileMemoryStore(), embeddingGenerator);

                var pdfQueries = _serviceProvider.GetRequiredService<PdfQueries>();
                var pdfText = await pdfQueries.GetData(connectionString);

                await memory.SaveInformationAsync(refData.Subject, id: refData.Subject, text: pdfText);

                var memoryPlugin = new TextMemoryPlugin(memory);

                _kernel.ImportPluginFromObject(memoryPlugin, refData.Subject);
            }

            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            };

            var prompt = await GetPrompt("memory_content");
            chatHistory.AddUserMessage(prompt);

            var arguments = new KernelArguments(settings)
                {
                    { "input", query },
                    { "collection", refData.Subject }
                };

            var response = await _kernel.InvokePromptAsync(prompt, arguments);
            chatHistory.AddAssistantMessage(response.ToString());

            return response.ToString()!;
        }
    }
}
