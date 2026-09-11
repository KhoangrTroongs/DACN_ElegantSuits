using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElegantSuits.Application.Common.Models;

[JsonConverter(typeof(PaginatedListJsonConverterFactory))]
public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public PaginatedList() { }

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalItems = count;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(count / (double)pageSize) : 0;

        AddRange(items);
    }

    public static PaginatedList<T> Create(IEnumerable<T> source, int count, int pageIndex, int pageSize)
    {
        var items = source.ToList();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}

public class PaginatedListJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType) return false;
        return typeToConvert.GetGenericTypeDefinition() == typeof(PaginatedList<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var itemType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(PaginatedListJsonConverter<>).MakeGenericType(itemType);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

public class PaginatedListJsonConverter<T> : JsonConverter<PaginatedList<T>>
{
    public override PaginatedList<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = JsonSerializer.Deserialize<List<T>>(ref reader, options) ?? new List<T>();
            return new PaginatedList<T>(list, list.Count, 1, Math.Max(1, list.Count));
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject or StartArray for PaginatedList.");
        }

        var items = new List<T>();
        int pageIndex = 1;
        int pageSize = 10;
        int totalItems = 0;
        int totalPages = 0;

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        foreach (var prop in root.EnumerateObject())
        {
            if (prop.NameEquals("items") || prop.NameEquals("Items"))
            {
                items = JsonSerializer.Deserialize<List<T>>(prop.Value.GetRawText(), options) ?? new List<T>();
            }
            else if (prop.NameEquals("pageIndex") || prop.NameEquals("PageIndex"))
            {
                pageIndex = prop.Value.GetInt32();
            }
            else if (prop.NameEquals("pageSize") || prop.NameEquals("PageSize"))
            {
                pageSize = prop.Value.GetInt32();
            }
            else if (prop.NameEquals("totalItems") || prop.NameEquals("TotalItems"))
            {
                totalItems = prop.Value.GetInt32();
            }
            else if (prop.NameEquals("totalPages") || prop.NameEquals("TotalPages"))
            {
                totalPages = prop.Value.GetInt32();
            }
        }

        var result = new PaginatedList<T>(items, totalItems > 0 ? totalItems : items.Count, pageIndex, pageSize);
        if (totalPages > 0) result.TotalPages = totalPages;
        return result;
    }

    public override void Write(Utf8JsonWriter writer, PaginatedList<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("PageIndex", value.PageIndex);
        writer.WriteNumber("PageSize", value.PageSize);
        writer.WriteNumber("TotalItems", value.TotalItems);
        writer.WriteNumber("TotalPages", value.TotalPages);
        writer.WriteBoolean("HasPreviousPage", value.HasPreviousPage);
        writer.WriteBoolean("HasNextPage", value.HasNextPage);
        writer.WritePropertyName("Items");
        JsonSerializer.Serialize(writer, (List<T>)value, options);
        writer.WriteEndObject();
    }
}
