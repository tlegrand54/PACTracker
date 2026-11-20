namespace PACTracker.Core.Energy.DTOs;

public sealed class EnergyDataFileDto
{
    public int Version { get; init; }

    public List<EnergyDataRecordDto>? Records { get; init; }
}
