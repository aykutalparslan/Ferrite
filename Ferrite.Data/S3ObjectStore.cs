// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace Ferrite.Data;

public class S3ObjectStore : IDistributedObjectStore
{
    private readonly AmazonS3Client _s3Client;
    private const string SmallFileBucketName = "ferrite-small-files";
    private const string BigFileBucketName = "ferrite-big-files";
    private bool _bucketsInitialized = false;
    private readonly Task _createBuckets;
    public S3ObjectStore(string serviceUrl, string accessKey, string secretKey)
    {
        var config = new AmazonS3Config
        {
            AuthenticationRegion = RegionEndpoint.USEast1.SystemName, // Should match the `MINIO_REGION` environment variable.
            ServiceURL = serviceUrl, // replace http://localhost:9000 with URL of your MinIO server
            ForcePathStyle = true // MUST be true to work correctly with MinIO server
        };
        _s3Client = new AmazonS3Client(
            accessKey,
            secretKey,
            config
        );
        
        _createBuckets = CreateBuckets();
    }

    private async Task CreateBuckets()
    {
        var buckets = await _s3Client.ListBucketsAsync();
        if (!buckets.Buckets.Select(_ => _.BucketName == SmallFileBucketName).Any())
        {
            PutBucketRequest request = new PutBucketRequest();
            request.BucketName = SmallFileBucketName;
            var res = await _s3Client.PutBucketAsync(request);
        }
        if (!buckets.Buckets.Select(_ => _.BucketName == BigFileBucketName).Any())
        {
            PutBucketRequest request = new PutBucketRequest();
            request.BucketName = BigFileBucketName;
            var res = await _s3Client.PutBucketAsync(request);
        }

        _bucketsInitialized = true;
    }

    public async Task<bool> SaveFilePart(long fileId, int filePart, Stream data)
    {
        if (!_bucketsInitialized)
        {
            await _createBuckets;
        }
        PutObjectRequest putObjectRequest = new PutObjectRequest();
        putObjectRequest.InputStream = data;
        putObjectRequest.Key = fileId.ToString("X")+"-"+filePart.ToString("X");
        putObjectRequest.BucketName = SmallFileBucketName;
        var putObjectResponse = await _s3Client.PutObjectAsync(putObjectRequest);
        return true;
    }

    public async Task<bool> SaveBigFilePart(long fileId, int filePart, int fileTotalParts, Stream data)
    {
        if (!_bucketsInitialized)
        {
            await _createBuckets;
        }
        PutObjectRequest putObjectRequest = new PutObjectRequest();
        putObjectRequest.InputStream = data;
        putObjectRequest.Key = fileId.ToString("X")+"-"+filePart.ToString("X");
        putObjectRequest.BucketName = BigFileBucketName;
        var putObjectResponse = await _s3Client.PutObjectAsync(putObjectRequest);
        return true;
    }

    public async Task<Stream> GetFilePart(long fileId, int filePart)
    {
        //get file part from s3 and return as a stream
        GetObjectRequest getObjectRequest = new GetObjectRequest();
        getObjectRequest.Key = fileId.ToString("X")+"-"+filePart.ToString("X");
        getObjectRequest.BucketName = SmallFileBucketName;
        var getObjectResponse = await _s3Client.GetObjectAsync(getObjectRequest);
        return getObjectResponse.ResponseStream;
    }

    public async Task<Stream> GetBigFilePart(long fileId, int filePart)
    {
        //get big file part from s3 and return as a stream
        GetObjectRequest getObjectRequest = new GetObjectRequest();
        getObjectRequest.Key = fileId.ToString("X")+"-"+filePart.ToString("X");
        getObjectRequest.BucketName = BigFileBucketName;
        var getObjectResponse = await _s3Client.GetObjectAsync(getObjectRequest);
        return getObjectResponse.ResponseStream;
    }
}