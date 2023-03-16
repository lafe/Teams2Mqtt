using lafe.Teams2Mqtt.Attributes;
namespace lafe.Teams2Mqtt.Mqtt.ComponentConfiguration;

public enum SensorDeviceClass
{
    /// <summary>
    /// Generic sensor. This is the default and doesn't need to be set.
    /// </summary>
    [Name("None")]
    None,
    /// <summary>
    /// Apparent power in VA.
    /// </summary>
    [Name("apparent_power")]
    ApparentPower,
    /// <summary>
    /// Air Quality Index
    /// </summary>
    [Name("aqi")]
    Aqi,
    /// <summary>
    /// Atmospheric pressure in cbar, bar, hPa, inHg, kPa, mbar, Pa, psi
    /// </summary>
    [Name("atmospheric_pressure")]
    AtmosphericPressure,
    /// <summary>
    /// Percentage of battery that is left
    /// </summary>
    [Name("battery")]
    Battery,
    /// <summary>
    /// Carbon Dioxide in CO2 (Smoke)
    /// </summary>
    [Name("carbon_dioxide")]
    CarbonDioxide,
    /// <summary>
    /// Carbon Monoxide in CO (Gas CNG/LPG)
    /// </summary>
    [Name("carbon_monoxide")]
    CarbonMmonoxide,
    /// <summary>
    /// Current in A, mA
    /// </summary>
    [Name("current")]
    Current,
    /// <summary>
    /// Data rate in bit/s, kbit/s, Mbit/s, Gbit/s, B/s, kB/s, MB/s, GB/s, KiB/s, MiB/s, or GiB/s
    /// </summary>
    [Name("data_rate")]
    DataRate,
    /// <summary>
    /// Data size in bit, kbit, Mbit, Gbit, B, kB, MB, GB, TB, PB, EB, ZB, YB, KiB, MiB, GiB, TiB, PiB, EiB, ZiB, or YiB
    /// </summary>
    [Name("data_size")]
    DataSize,
    /// <summary>
    /// Date string (ISO 8601)
    /// </summary>
    [Name("date")]
    Date,
    /// <summary>
    /// Generic distance in km, m, cm, mm, mi, yd, or in
    /// </summary>
    [Name("distance")]
    Distance,
    /// <summary>
    /// Duration in days, hours, minutes or seconds
    /// </summary>
    [Name("duration")]
    Duration,
    /// <summary>
    /// Energy in Wh, kWh or MWh
    /// </summary>
    [Name("energy")]
    Energy,
    /// <summary>
    /// Has a limited set of (non-numeric) states
    /// </summary>
    [Name("enum")]
    Enumeration,
    /// <summary>
    /// Frequency in Hz, kHz, MHz or GHz
    /// </summary>
    [Name("frequency")]
    Frequency,
    /// <summary>
    /// Gasvolume in m³, ft³, or CCF
    /// </summary>
    [Name("gas")]
    Gas,
    /// <summary>
    /// Percentage of humidity in the air
    /// </summary>
    [Name("humidity")]
    Humidity,
    /// <summary>
    /// The current light level in lx
    /// </summary>
    [Name("illuminance")]
    Illuminance,
    /// <summary>
    /// Irradiance in W/m² or BTU/(h⋅ft²)
    /// </summary>
    [Name("irradiance")]
    Irradiance,
    /// <summary>
    /// Percentage of water in a substance
    /// </summary>
    [Name("moisture")]
    Moisture,
    /// <summary>
    /// The monetary value
    /// </summary>
    [Name("monetary")]
    Monetary,
    /// <summary>
    /// Concentration of Nitrogen Dioxide in µg/m³
    /// </summary>
    [Name("nitrogen_dioxide")]
    NitrogenDioxide,
    /// <summary>
    /// Concentration of Nitrogen Monoxide in µg/m³
    /// </summary>
    [Name("nitrogen_monoxide")]
    NitrogenMonoxide,
    /// <summary>
    /// Concentration of Nitrous Oxide in µg/m³
    /// </summary>
    [Name("nitrous_oxide")]
    NitrousOxide,
    /// <summary>
    /// Concentration of Ozone in µg/m³
    /// </summary>
    [Name("ozone")]
    Ozone,
    /// <summary>
    /// Concentration of particulate matter less than 1 micrometer in µg/m³
    /// </summary>
    [Name("pm1")]
    Om1,
    /// <summary>
    /// Concentration of particulate matter less than 10 micrometers in µg/m³
    /// </summary>
    [Name("pm10")]
    Pm10,
    /// <summary>
    /// Concentration of particulate matter less than 2.5 micrometers in µg/m³
    /// </summary>
    [Name("pm25")]
    Pm25,
    /// <summary>
    /// Power factor in %
    /// </summary>
    [Name("power_factor")]
    PowerFactor,
    /// <summary>
    /// Power in W or kW
    /// </summary>
    [Name("power")]
    Power,
    /// <summary>
    /// Accumulated precipitation in cm, in or mm
    /// </summary>
    [Name("precipitation")]
    Precipitation,
    /// <summary>
    /// Precipitation intensity in in/d, in/h, mm/d, or mm/h
    /// </summary>
    [Name("precipitation_intensity")]
    PrecipitationIntensity,
    /// <summary>
    /// Pressure in Pa, kPa, hPa, bar, cbar, mbar, mmHg, inHg, or psi
    /// </summary>
    [Name("pressure")]
    Pressure,
    /// <summary>
    /// Reactive power in var
    /// </summary>
    [Name("reactive_power")]
    ReactivePower,
    /// <summary>
    /// Signal strength in dB or dBm
    /// </summary>
    [Name("signal_strength")]
    SignalStrength,
    /// <summary>
    /// Sound pressure in dB or dBA
    /// </summary>
    [Name("sound_pressure")]
    SoundPressure,
    /// <summary>
    /// Generic speed in ft/s, in/d, in/h, km/h, kn, m/s, mph, or mm/d
    /// </summary>
    [Name("speed")]
    Speed,
    /// <summary>
    /// Concentration of sulphur dioxide in µg/m³
    /// </summary>
    [Name("sulphur_dioxide")]
    SulphurDioxide,
    /// <summary>
    /// Temperature in °C or °F
    /// </summary>
    [Name("temperature")]
    Temperature,
    /// <summary>
    /// Datetime object or timestamp string (ISO 8601)
    /// </summary>
    [Name("timestamp")]
    Timestamp,
    /// <summary>
    /// Concentration of volatile organic compounds in µg/m³
    /// </summary>
    [Name("volatile_organic_compounds")]
    VolatileOrganicCompounds,
    /// <summary>
    /// Voltage in V, mV
    /// </summary>
    [Name("voltage")]
    Voltage,
    /// <summary>
    /// Generic volume in L, mL, gal, fl. oz., m³, ft³, or CCF
    /// </summary>
    [Name("volume")]
    Volume,
    /// <summary>
    /// Water consumption in L, gal, m³, ft³, or CCF
    /// </summary>
    [Name("water")]
    Water,
    /// <summary>
    /// Generic mass in kg, g, mg, µg, oz, lb, or st
    /// </summary>
    [Name("weight")]
    Weight,
    /// <summary>
    /// Wind speed in ft/s, km/h, kn, m/s, or mph
    /// </summary>
    [Name("wind_speed")]
    WindSpeed,
}