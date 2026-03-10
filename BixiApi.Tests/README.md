# BixiApi.Tests — xUnit Test Suite

Unit tests for the BixiApi backend. Tests are kept in a separate project so the production assembly ships with zero test dependencies.

## Prerequisites

Same as BixiApi — .NET 9 must be in your PATH. See `../BixiApi/README.md`.

## Run

```bash
dotnet test
```

## Project Structure

```
BixiApi.Tests/
└── ServiceTests/
    └── DistanceServiceTests.cs   # Haversine formula correctness tests
```

## Test Coverage

### DistanceServiceTests

| Test | What it proves |
|------|----------------|
| `SamePoint_ReturnsZero` | Formula returns exactly 0 when both points are identical — basic sanity check |
| `WorkleapToMetroLaurier_IsApproximately5100m` | Real-world accuracy against known Montreal coordinates |
| `OneDegreeNorth_IsApproximately111km` | One degree of latitude ≈ 111 km — a well-known geographic constant |
| `TwoNearbyPoints_AreApproximately500mApart` | Short-range accuracy; catches unit scale errors (km vs m) |
| `Distance_IsSymmetric` | A→B equals B→A within floating-point tolerance — a mathematical property of Haversine |

## Design Notes

- Tests are named `Method_Scenario_ExpectedResult` for readability at a glance.
- `_sut` (System Under Test) is a common unit-testing convention — it makes clear which class is being exercised.
- `Assert.InRange` is used instead of exact equality for geographic distances because small floating-point differences are expected and acceptable.
