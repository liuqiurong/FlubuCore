﻿using Flubu.Context;
using Flubu.Tasks.Text;
using Xunit;

namespace Flubu.Tests.Tasks
{
    [Collection(nameof(TaskTestCollection))]
    public class UpdateJsonFileTaskTests : TaskTestBase
    {
        private readonly TaskTestFixture _fixture;

        public UpdateJsonFileTaskTests(TaskTestFixture fixture)
            : base(fixture.LoggerFactory)
        {
            _fixture = fixture;
        }

        [Fact]
        public void MissingFile()
        {
            UpdateJsonFileTask task = new UpdateJsonFileTask("nonext.json");

            TaskExecutionException e = Assert.Throws<TaskExecutionException>(() => task.Execute(Context));

            Assert.Equal("JSON file nonext.json not found!", e.Message);
            Assert.Equal(1, e.ErrorCode);
        }

        [Fact]
        public void MissingUpdates()
        {
            UpdateJsonFileTask task = new UpdateJsonFileTask("TestData/testproject.json".ExpandToExecutingPath());

            TaskExecutionException e = Assert.Throws<TaskExecutionException>(() => task.Execute(Context));

            Assert.StartsWith("Nothing to update in file", e.Message);
            Assert.Equal(2, e.ErrorCode);
        }

        [Fact]
        public void FailOnUpdateNotFound()
        {
            UpdateJsonFileTask task = new UpdateJsonFileTask("TestData/testproject.json".ExpandToExecutingPath());
            task.Update("notfoundproperty", "test");

            TaskExecutionException e = Assert.Throws<TaskExecutionException>(() => task.Execute(Context));

            Assert.StartsWith("Propety notfoundproperty not found in", e.Message);
            Assert.Equal(3, e.ErrorCode);
        }

        [Fact]
        public void FailOnTypeMissmatch()
        {
            UpdateJsonFileTask task = new UpdateJsonFileTask("TestData/testproject.json".ExpandToExecutingPath());
            task
                .FailOnTypeMismatch(true)
                .Update("version", 1);

            TaskExecutionException e = Assert.Throws<TaskExecutionException>(() => task.Execute(Context));

            Assert.StartsWith("Propety version type mismatch.", e.Message);
            Assert.Equal(4, e.ErrorCode);
        }

        [Fact]
        public void DontFailOnUpdateNotFound()
        {
            UpdateJsonFileTask task = new UpdateJsonFileTask("TestData/testproject.json".ExpandToExecutingPath());
            task
                .FailIfPropertyNotFound(false)
                .Update("notfoundproperty", "test");

            int res = task.Execute(Context);
            Assert.Equal(3, res);
        }

        [Fact]
        public void UpdateSucess()
        {
            UpdateJsonFileTask task = new UpdateJsonFileTask("TestData/testproject.json".ExpandToExecutingPath());

            int res = task
                .Update("version", "2.0.0.0")
                .Output("project.json.new")
                .Execute(Context);

            Assert.Equal(0, res);
        }

        [Fact]
        public void UpdateSuccessTypeMismatch()
        {
            UpdateJsonFileTask task = new UpdateJsonFileTask("TestData/testproject.json".ExpandToExecutingPath());

            int res = task
                .Update("version", 2)
                .Output("project.json.new")
                .Execute(Context);

            Assert.Equal(4, res);
        }
    }
}