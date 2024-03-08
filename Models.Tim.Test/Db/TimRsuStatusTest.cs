using Econolite.Ode.Models.Tim;
using Econolite.Ode.Models.Tim.Db;
using Econolite.Ode.Models.Tim.ItisCodes;
using Econolite.Ode.Models.Tim.Messaging;
using Econolite.Ode.TimService.Models;
using FluentAssertions;

namespace Models.Tim.Test.Db;

public class TimRsuStatusTest
{
    [Fact]
    public void TimDocument_AddToTimRsuStatus()
    {
        var intersectionId = Guid.Parse("19fe2cba-d5d0-405f-b7c1-4f72b14ac402");
        var rsuId = Guid.Parse("19fe2cba-d5d0-405f-b7c1-4f72b14ac403");
        var request1 = CreateTimRequest(1);
        var request2 = CreateTimRequest(2);
        var request3 = CreateTimRequest(3);
        var targetEntity = new TargetEntity()
        {
            IntersectionId = intersectionId,
            TargetId = rsuId,
            Location = new double[]{-104.821298, 38.915467},
            Region = new double[][][]{new []{new []{-104.821298, 38.915467 }, new []{-104.821298, 38.915467 }}}
        };
        
        // Arrange
        var timDocument = request1.ToTimDocument(targetEntity);
        var timDocument2 = request2.ToTimDocument(targetEntity);
        var timDocument3 = request3.ToTimDocument(targetEntity);
        
        // Act
        var timRsuStatus = timDocument.ToTimRsuStatus();
        timRsuStatus.AddTimDocument(timDocument2);
        timRsuStatus.AddTimDocument(timDocument3);
        timRsuStatus.ToRequest();
        // Assert
        timRsuStatus.Should().NotBeNull();
    }
    
    public TimRequest CreateTimRequest(int duration, Guid id = default)
    {
        var documentId = id == default ? Guid.NewGuid() : id;
        
        return new TimRequest()
        {
            Id = documentId,
            Cancel = false,
            ItisCode = ItisCode.SlowTraffic,
            DurationType = DurationType.Minutes,
            Duration = duration,
            Latitude = 38.915467,
            Longitude = -104.821298,
            MessageType = MessageType.Information,
            TransmitMode = TimTransmitMode.Alternating,
            TargetType = TargetType.Target,
            Parameters = new []{"19fe2cba-d5d0-405f-b7c1-4f72b14ac402"}
        };
    }
}