﻿namespace PSI_Project.Requests;

public class StudyTimeUpdateRequest
{
    public string SubjectId { get; set; }
    public double ElapsedHours { get; set; }
}