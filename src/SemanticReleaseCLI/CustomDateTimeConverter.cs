using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SemanticReleaseCLI;

public sealed class CustomDateTimeConverter(string format) : JsonConverter<DateTime>
{
	#region Private Members

	private readonly string _format = format;

	#endregion Private Members
	
	#region Public Methods
    
	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		=> writer.WriteStringValue(value.ToString(_format));

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> DateTime.Parse(reader.GetString() ?? throw new ArgumentNullException(nameof(reader)), CultureInfo.CurrentCulture);
	
	#endregion Public Methods
}