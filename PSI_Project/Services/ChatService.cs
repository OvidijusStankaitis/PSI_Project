﻿using PSI_Project.Models;
using PSI_Project.Repositories;

namespace PSI_Project.Services;

public class ChatService
{
    private readonly CommentRepository _commentRepository;
    private readonly ILogger<ChatService> _logger; 

    public ChatService(CommentRepository commentRepository, ILogger<ChatService> logger)
    {
        _logger = logger; 
        _commentRepository = commentRepository;
    }
    
    public Comment? SaveSentMessage(string userId, string topicId, string message)
    {
        // Check if User exists
        var userExists = _commentRepository.EduPalContext.Users.Any(u => u.Id == userId);
        if (!userExists)
        {
            // Log the error or handle it as per your error handling policy
            _logger.LogError("User with ID {UserId} does not exist.", userId);
            return null; // or throw an exception as per your design
        }

        Comment newComment = new Comment(userId, topicId, message);
        _commentRepository.Add(newComment);
        _commentRepository.EduPalContext.SaveChanges();

        return newComment;
    }

    public Comment DeleteMessage(string messageId)
    {
        Comment message = _commentRepository.Get(messageId);
        _commentRepository.Remove(message);
        _commentRepository.EduPalContext.SaveChanges();
        
        return message;
    }
}