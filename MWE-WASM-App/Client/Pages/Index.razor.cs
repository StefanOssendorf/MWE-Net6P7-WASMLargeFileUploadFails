
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace MWE_WASM_App.Client.Pages;
public partial class Index
{
    [Inject]
    public ILogger<Index> Logger { get; set; }

    [Inject]
    public HttpClient Http { get; set; }


    public Boolean? IsUploaded { get; set; }

    private async Task LoadFiles(InputFileChangeEventArgs e)
    {
        IsUploaded = null;

        try
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(e.File.OpenReadStream(e.File.Size));

            content.Add(content: fileContent, name: "\"files\"", fileName: e.File.Name);


            var response = await Http.PostAsync("/Filesave", content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            IsUploaded = true;
        }
        catch (Exception exc)
        {
            IsUploaded = false;
            Logger.LogError(exc, "Error during upload");
        }
    }
}
