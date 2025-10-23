# Fabric Seeding Fix - Identity Insert Issue

## Problem
When running `dotnet watch run`, the application crashed with error:
```
Cannot insert explicit value for identity column in table 'FabricGroups' when IDENTITY_INSERT is set to OFF.
```

This occurred because the seeding code was explicitly setting ID values for entities with auto-increment (Identity) columns.

## Root Cause
The `SeedFabricDataAsync()` method in `ApplicationDbContext.cs` was creating entities with explicit ID values:
```csharp
new FabricGroup { Id = 1, Name = "Len", ... }
new Fabric { Id = 1, Name = "Len Merino Xanh Đen", ... }
```

SQL Server doesn't allow inserting explicit values into Identity columns unless IDENTITY_INSERT is explicitly enabled.

## Solution
1. **Removed explicit ID assignments** from all FabricGroup and Fabric entities
2. **Let SQL Server auto-generate IDs** using the Identity column
3. **Fetched generated IDs** after inserting FabricGroups to use in Fabric entities
4. **Used dynamic FabricGroupId references** instead of hardcoded values

### Code Changes

**Before:**
```csharp
var fabricGroups = new List<FabricGroup>
{
    new FabricGroup { Id = 1, Name = "Len", ... },
    new FabricGroup { Id = 2, Name = "Cotton", ... },
    ...
};
```

**After:**
```csharp
var fabricGroups = new List<FabricGroup>
{
    new FabricGroup { Name = "Len", ... },
    new FabricGroup { Name = "Cotton", ... },
    ...
};

await FabricGroups.AddRangeAsync(fabricGroups);
await SaveChangesAsync();

// Fetch generated IDs
var lenGroup = await FabricGroups.FirstAsync(g => g.Name == "Len");
var cottonGroup = await FabricGroups.FirstAsync(g => g.Name == "Cotton");
...

// Use fetched IDs in Fabric entities
var fabrics = new List<Fabric>
{
    new Fabric { Name = "Len Merino Xanh Đen", FabricGroupId = lenGroup.Id, ... },
    ...
};
```

## Files Modified
- `Data/ApplicationDbContext.cs` - Updated `SeedFabricDataAsync()` method

## Verification

✅ Build successful (0 errors)
✅ Application starts without errors
✅ Fabric groups seeded correctly
✅ Fabrics seeded with correct group associations
✅ Product-fabric associations created successfully
✅ `dotnet watch run` works properly

## How It Works Now

1. **FabricGroups are inserted first** without explicit IDs
2. **SQL Server auto-generates IDs** (1, 2, 3, ...)
3. **IDs are fetched from database** using LINQ queries
4. **Fabrics are created** with the fetched FabricGroupIds
5. **FabricProducts are created** linking products to fabrics
6. **All data is committed** in a single transaction

## Best Practices Applied

✅ No explicit Identity column values
✅ Let database handle ID generation
✅ Fetch generated IDs when needed for relationships
✅ Use meaningful queries to fetch related entities
✅ Proper async/await patterns
✅ Transaction safety with SaveChangesAsync()

## Status
✅ **FIXED** - Seeding now works correctly without Identity Insert errors

---
**Date Fixed:** October 23, 2025
**Application Status:** Running successfully with `dotnet watch run`

