﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.TestStubs;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Infrastructure
{
    public class AlertsRepositoryTests
    {
        private Mock<IConfigurationProvider> _configurationProviderMock;
        private readonly Mock<IBlobStorageClient> _blobStorageClientMock;
        private readonly AlertsRepository alertsRepository;
        private readonly IFixture fixture;

        public AlertsRepositoryTests()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _blobStorageClientMock = new Mock<IBlobStorageClient>();
            var blobStorageFactory = new BlobStorageClientFactory(_blobStorageClientMock.Object);
            _configurationProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            alertsRepository = new AlertsRepository(_configurationProviderMock.Object, blobStorageFactory);
        }

        [Fact]
        public async void LoadLatestAlertHistoryAsyncTest()
        {
            int year = 2016;
            int month = 7;
            int date = 5;
            string value = "10.0";
            DateTime minTime = new DateTime(year, month, date);

            await
                Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                    async () => await alertsRepository.LoadLatestAlertHistoryAsync(minTime, 0));

            
            _blobStorageClientMock.Setup(x => x.GetReader(It.IsNotNull<string>(), null))
                .ReturnsAsync(new AlertHistoryBlobReaderStub(year, month, date, value));
            var alertsList = await alertsRepository.LoadLatestAlertHistoryAsync(minTime, 5);
            Assert.NotNull(alertsList);
            Assert.NotEmpty(alertsList);
            Assert.Equal(alertsList.First().Value, value);
            Assert.Equal(alertsList.First().Timestamp, minTime);
        }
    }
}