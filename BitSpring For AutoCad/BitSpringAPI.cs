using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RestSharp;

namespace BitSpring_For_AutoCad
{
    class BitSpringAPI
    {
        private RestClient mClient;
        private String mSession;

        public BitSpringAPI()
        {
            mClient = new RestClient("http://newsendit.hightail.com:8080/api");
        }

        public String CreateNewSpace(String fileURL, String fileName)
        {
            //Ensure that we have an API session
            GetSession();
            //Create the space
            Space space = PostSpace(fileURL);
            //Upload the file
            UploadFile(space, fileURL, fileName);
            //Return the space URL
            return "http://newsendit.hightail.com:3000/space/" + space.url;
        }

        private void GetSession()
        {
            var request = new RestRequest("/v1/auth/session", Method.GET);
            mSession = mClient.Execute(request).Content;
        }

        private Space PostSpace(String fileURL)
        {
            //Return the space
            var request = new RestRequest("/v1/spaces", Method.POST);
            request.AddHeader("Authorization", mSession);
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new { });
            return mClient.Execute<Space>(request).Data;
        }

        private void PostStatus(String fileName, long fileSize, long sizeUploaded, long secondsRemaining)
        {
            //Create the upload status object to use for serialization of the status
            UploadStatus status = new UploadStatus();
            status.bytesTotal = fileSize;
            status.bytesUploaded = sizeUploaded;
            status.fileName = fileName;
            status.secondsRemaining = secondsRemaining;
            //Create the status request
            var request = new RestRequest("/v1/upload/status/", Method.POST);
            request.AddHeader("Authorization", mSession);
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = DataFormat.Json;
            request.AddBody(status);
            mClient.Execute(request);
        }

        private void UploadFile(Space space, String fileURL, String fileName)
        {
            //Get the file size
            long fileSize = (new FileInfo(fileURL)).Length;
            //Send the initial status
            PostStatus(fileName, fileSize, 0, Int64.MaxValue);
            //Open the file as a stream
            FileStream fileStream = new FileStream(fileURL, FileMode.Open);
            //Upload the file
            var request = new RestRequest("/v1/upload/" + space.id, Method.POST);
            request.AddHeader("Authorization", mSession);
            request.AddFile("file", ReadFileToEnd(fileStream), fileName + ".pdf");
            mClient.Execute(request);
            fileStream.Close();
            //Send the complete status
            PostStatus(fileName, fileSize, fileSize, 0);
        }

        public byte[] ReadFileToEnd(System.IO.Stream stream)
        {
            long originalPosition = stream.Position;
            stream.Position = 0;

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;
                
                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }
    }
}
