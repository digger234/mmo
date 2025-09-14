using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using RestSharp;
using Newtonsoft.Json;

namespace MMOTool.Core
{
    public class JobSystem : IDisposable
    {
        private List<JobPlatform> _platforms;
        private System.Timers.Timer _jobCheckTimer;
        private bool _isRunning;
        private double _totalEarnings;
        private int _todayJobs;
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<JobInfo> JobCompleted;
        public event EventHandler<double> EarningsUpdated;
        
        public bool IsRunning => _isRunning;
        public double TotalEarnings => _totalEarnings;
        public int TodayJobs => _todayJobs;
        
        public JobSystem()
        {
            _platforms = new List<JobPlatform>();
            _totalEarnings = 0.0;
            _todayJobs = 0;
        }
        
        public async Task Initialize()
        {
            try
            {
                await LoadJobPlatforms();
                SetupJobCheckTimer();
                LogMessage?.Invoke(this, "JobSystem initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error initializing JobSystem: {ex.Message}");
                throw;
            }
        }
        
        private async Task LoadJobPlatforms()
        {
            _platforms.Clear();
            
            var platforms = new List<JobPlatform>
            {
                new JobPlatform
                {
                    Name = "Golike",
                    ApiUrl = "https://api.golike.com",
                    IsEnabled = true,
                    ApiKey = "your_golike_api_key",
                    MinEarnings = 0.01,
                    MaxEarnings = 0.50
                },
                new JobPlatform
                {
                    Name = "Traodoisub",
                    ApiUrl = "https://api.traodoisub.com",
                    IsEnabled = true,
                    ApiKey = "your_traodoisub_api_key",
                    MinEarnings = 0.005,
                    MaxEarnings = 0.25
                },
                new JobPlatform
                {
                    Name = "Tuongtaccheo",
                    ApiUrl = "https://api.tuongtaccheo.com",
                    IsEnabled = true,
                    ApiKey = "your_tuongtaccheo_api_key",
                    MinEarnings = 0.01,
                    MaxEarnings = 0.30
                }
            };
            
            _platforms.AddRange(platforms);
            LogMessage?.Invoke(this, $"Loaded {_platforms.Count} job platforms");
            await Task.CompletedTask;
        }
        
        private void SetupJobCheckTimer()
        {
            _jobCheckTimer = new System.Timers.Timer(60000);
            _jobCheckTimer.Elapsed += OnJobCheckTimerElapsed;
        }
        
        private async void OnJobCheckTimerElapsed(object sender, ElapsedEventArgs e)
        {
            await CheckForJobs();
        }
        
        public Task Start()
        {
            if (_isRunning) return Task.CompletedTask;
            
            _isRunning = true;
            _jobCheckTimer.Start();
            LogMessage?.Invoke(this, "JobSystem started");
            return Task.CompletedTask;
        }
        
        public Task Stop()
        {
            if (!_isRunning) return Task.CompletedTask;
            
            _isRunning = false;
            _jobCheckTimer.Stop();
            LogMessage?.Invoke(this, "JobSystem stopped");
            return Task.CompletedTask;
        }
        
        private async Task CheckForJobs()
        {
            foreach (var platform in _platforms)
            {
                if (!platform.IsEnabled) continue;
                
                try
                {
                    var jobs = await GetAvailableJobs(platform);
                    foreach (var job in jobs)
                    {
                        await ProcessJob(job, platform);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke(this, $"Error checking jobs on {platform.Name}: {ex.Message}");
                }
            }
        }
        
        private async Task<List<JobInfo>> GetAvailableJobs(JobPlatform platform)
        {
            var jobs = new List<JobInfo>();
            
            try
            {
                var client = new RestClient(platform.ApiUrl);
                var request = new RestRequest("/api/jobs/available", Method.Get);
                request.AddHeader("Authorization", $"Bearer {platform.ApiKey}");
                
                var response = await client.ExecuteAsync(request);
                
                if (response.IsSuccessful)
                {
                    var jobData = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    if (jobData?.jobs != null)
                    {
                        foreach (var job in jobData.jobs)
                        {
                            jobs.Add(new JobInfo
                            {
                                Id = job.id,
                                Platform = platform.Name,
                                Title = job.title,
                                Description = job.description,
                                Reward = job.reward,
                                Type = job.type,
                                Status = "Available"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error getting jobs from {platform.Name}: {ex.Message}");
            }
            
            return jobs;
        }
        
        private async Task ProcessJob(JobInfo job, JobPlatform platform)
        {
            try
            {
                var client = new RestClient(platform.ApiUrl);
                var request = new RestRequest($"/api/jobs/{job.Id}/accept", Method.Post);
                request.AddHeader("Authorization", $"Bearer {platform.ApiKey}");
                
                var response = await client.ExecuteAsync(request);
                
                if (response.IsSuccessful)
                {
                    job.Status = "Completed";
                    _totalEarnings += job.Reward;
                    _todayJobs++;
                    
                    JobCompleted?.Invoke(this, job);
                    EarningsUpdated?.Invoke(this, _totalEarnings);
                    
                    LogMessage?.Invoke(this, $"Completed job: {job.Title} - Reward: ${job.Reward:F2}");
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error processing job {job.Title}: {ex.Message}");
            }
        }
        
        public async Task<List<JobInfo>> GetJobHistory()
        {
            var history = new List<JobInfo>();
            
            foreach (var platform in _platforms)
            {
                if (!platform.IsEnabled) continue;
                
                try
                {
                    var client = new RestClient(platform.ApiUrl);
                    var request = new RestRequest("/api/jobs/history", Method.Get);
                    request.AddHeader("Authorization", $"Bearer {platform.ApiKey}");
                    
                    var response = await client.ExecuteAsync(request);
                    
                    if (response.IsSuccessful)
                    {
                        var jobData = JsonConvert.DeserializeObject<dynamic>(response.Content);
                        if (jobData?.jobs != null)
                        {
                            foreach (var job in jobData.jobs)
                            {
                                history.Add(new JobInfo
                                {
                                    Id = job.id,
                                    Platform = platform.Name,
                                    Title = job.title,
                                    Description = job.description,
                                    Reward = job.reward,
                                    Type = job.type,
                                    Status = job.status,
                                    CompletedAt = job.completed_at
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke(this, $"Error getting job history from {platform.Name}: {ex.Message}");
                }
            }
            
            return history;
        }
        
        public int GetTodayJobs()
        {
            return _todayJobs;
        }
        
        public double GetTotalEarnings()
        {
            return _totalEarnings;
        }
        
        public void Dispose()
        {
            _jobCheckTimer?.Dispose();
        }
    }
    
    public class JobPlatform
    {
        public string Name { get; set; }
        public string ApiUrl { get; set; }
        public bool IsEnabled { get; set; }
        public string ApiKey { get; set; }
        public double MinEarnings { get; set; }
        public double MaxEarnings { get; set; }
    }
    
    public class JobInfo
    {
        public string Id { get; set; }
        public string Platform { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Reward { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}