﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.FileProviders;
using PSI_Project.Data;
using PSI_Project.DTO;
using PSI_Project.Exceptions;
using PSI_Project.Models;

namespace PSI_Project.Repositories;

public class ConspectusRepository : Repository<Conspectus>
{
    public EduPalDatabaseContext EduPalContext => Context as EduPalDatabaseContext;
    private readonly SemaphoreSlim _deleteLock = new SemaphoreSlim(1);

    public ConspectusRepository(EduPalDatabaseContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Conspectus>> GetConspectusListByTopicIdAsync(string topicId)
    {
        return await FindAsync(conspectus => conspectus.Topic.Id == topicId);
    }

    public async Task<Stream> GetPdfStreamAsync(string conspectusId)
    {
        Conspectus conspectus = await GetAsync(conspectusId);
        string dirPath = Path.GetDirectoryName(conspectus.Path)!;
        string filename = Path.GetFileName(conspectus.Path);

        IFileProvider provider = new PhysicalFileProvider(dirPath);
        IFileInfo fileInfo = provider.GetFileInfo(filename);

        return fileInfo.CreateReadStream();
    }

    public async Task<ConspectusFileContentDTO> DownloadAsync(string conspectusId)
    {
        Conspectus conspectus = await GetAsync(conspectusId);
        byte[] fileBytes = await File.ReadAllBytesAsync(conspectus.Path);
        FileContentResult fileContent = new FileContentResult(fileBytes, "application/pdf")
        {
            FileDownloadName = Path.GetFileName(conspectus.Name)
        };

        return new ConspectusFileContentDTO(conspectus.Name, fileContent);
    }

    public async Task<IEnumerable<Conspectus>> UploadAsync(string topicId, List<IFormFile> files)
    {
        var uploadedConspectuses = new List<Conspectus>();

        foreach (var formFile in files)
        {
            string fileName = formFile.FileName;
            if (!fileName.IsValidFileName())
            {
                Console.WriteLine($"The file {fileName} is not a valid PDF format.");
                continue;
            }

            string filePath;
            try
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(fileStream); // Use asynchronous method
                }
            }
            catch (Exception ex)
            {
                // Create at least 1 exception type and throw it; meaningfully deal with it; 
                throw new EntityCreationException("Error occurred while uploading one of the files", ex);
            }

            try
            {
                Topic? topic = await EduPalContext.Topics.FindAsync(topicId); // Use asynchronous FindAsync
                if (topic == null)
                {
                    // Create at least 1 exception type and throw it; meaningfully deal with it; 
                    throw new ObjectNotFoundException("Couldn't find topic with specified id");
                }

                Conspectus conspectus = new()
                {
                    Name = fileName,
                    Path = filePath,
                    Topic = topic
                };

                Add(conspectus);
                //await EduPalContext.SaveChangesAsync(); // Use asynchronous SaveChangesAsync
                uploadedConspectuses.Add(conspectus);
            }
            catch (Exception ex)
            {
                File.Delete(filePath);
                // Create at least 1 exception type and throw it; meaningfully deal with it; 
                throw new EntityCreationException("Error occurred while uploading one of the files", ex);
            }
        }

        return uploadedConspectuses;
    }

    public async Task<Conspectus> ChangeRatingAsync(string conspectusId, bool toIncrease)
    {
        Conspectus conspectus = await GetAsync(conspectusId);
        conspectus.Rating += toIncrease ? +1 : -1;

        await EduPalContext.SaveChangesAsync();
        return conspectus;
    }

    public async Task RemoveAsync(string conspectusId)
    {
        Conspectus conspectus = await GetAsync(conspectusId);

        // Use SemaphoreSlim for asynchronous thread safety
        await _deleteLock.WaitAsync();
        try
        {
            string filePath = conspectus.Path;

            // Remove the conspectus from the context
            Remove(conspectus);
            //await EduPalContext.SaveChangesAsync();

            // Try to delete the file
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"File deleted: {filePath}");
            }
            else
            {
                Console.WriteLine($"File not found: {filePath}");
            }
        }
        catch (Exception ex)
        {
            EntityEntry<Conspectus> entry = EduPalContext.Entry(conspectus);
            entry.State = EntityState.Unchanged;
            throw new EntityDeletionException("Couldn't delete conspectus", ex);
        }
        finally
        {
            _deleteLock.Release(); // Release the semaphore
        }
    }
}