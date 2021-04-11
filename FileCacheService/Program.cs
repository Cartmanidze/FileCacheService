using FileCacheService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

var extensions = new[] { ".txt", ".ini", ".exe" };
List<BackgroundWorker> workers = new() { new BackgroundWorker(), new BackgroundWorker(), new BackgroundWorker()};
workers.ForEach(w => w.DoWork += WorkerDoWork);
var count = 0;
foreach (var worker in workers.Where(worker => !worker.IsBusy))
{
    worker.RunWorkerAsync(extensions[count]);
    count++;
}
Console.ReadKey();

static void WorkerDoWork(object sender, DoWorkEventArgs e)
{
    while (true)
    {
        var files = FileCacheStore.Get((string)e.Argument);
        foreach (var file in files)
        {
            Console.WriteLine(file + " " + $"ThreadId = {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(1000);
        }       
    }
}

