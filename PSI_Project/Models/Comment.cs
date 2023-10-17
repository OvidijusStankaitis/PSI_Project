﻿namespace PSI_Project.Models;

public class Comment : BaseEntity 
{
    public string TopicId { get; init; }
    public string CommentText { get; init; } 

    public Comment(string topicId, string commentText) : base()
    {
        TopicId = topicId;
        CommentText = commentText;
    }
}