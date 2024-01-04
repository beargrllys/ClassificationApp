using ClassificationApp.Dto;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace ClassificationApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            RepictureBtn.IsVisible = false;
            PictureBtn.IsVisible = true;
            SendClassify.IsVisible = false;
        }

        private void OnTakePhotoClicked(object sender, EventArgs e)
        {
            TakePhoto();
        }
        public async void TakePhoto()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    // save the file into local storage
                    string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                    using Stream sourceStream = await photo.OpenReadAsync();
                    using (FileStream localFileStream = File.OpenWrite(localFilePath))
                    {
                        await sourceStream.CopyToAsync(localFileStream);
                    }

                    // Update the Image source
                    MyImage.Source = localFilePath;
                    ExplainLabel.Text = "다시 찍거나\n분류하기 버튼을 누르세요";
                    RepictureBtn.IsVisible = true;
                    PictureBtn.IsVisible = false;
                    SendClassify.IsVisible = true;
                }
            }
        }

        public async void OnButtonClick(object sender, EventArgs e)
        {
            if (MyImage.Source is FileImageSource fileImageSource)
            {
                await SendImageAsPostRequest(fileImageSource.File);
            }
            PictureBtn.IsVisible = true;
        }

        private async Task SendImageAsPostRequest(string imagePath)
        {
            var client = new HttpClient();
            var content = new MultipartFormDataContent();
            var imageBytes = await File.ReadAllBytesAsync(imagePath);
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            content.Add(imageContent, "file", Path.GetFileName(imagePath));
            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(string.Format("http://{0}:{1}/predict", "192.168.0.129", "5000"), content);
            }
            catch (Exception e)
            {
                ExplainLabel.Text = "전송중 문제가 발생했습니다. : " + e.Message;
                RepictureBtn.IsVisible = false;
                PictureBtn.IsVisible = false;
                SendClassify.IsVisible = true;
                return;
            }
            String? responseString = await response.Content.ReadAsStringAsync();
                ResultDTO? result = JsonConvert.DeserializeObject<ResultDTO>(responseString);
                if (result == null)
                {
                    ExplainLabel.Text = "분류중 문제가 발생했습니다.";
                    return;
                }
            string ret = result.result == "cat"? "고양이" : "개";
            ExplainLabel.Text = string.Format("분류 결과: {0} ({1}%)", ret, (float.Parse(result.confidence)).ToString("#.##"));
            RepictureBtn.IsVisible = true;
            PictureBtn.IsVisible = false;
            SendClassify.IsVisible = false;
        }

        private void RepictureBtn_Clicked(object sender, EventArgs e)
        {
            MyImage.Source = "";
            RepictureBtn.IsVisible = false;
            PictureBtn.IsVisible = true;
            SendClassify.IsVisible = false;
            ExplainLabel.Text = "판별이 필요한 \n 사진을 찍어주세요";
        }
    }

}
