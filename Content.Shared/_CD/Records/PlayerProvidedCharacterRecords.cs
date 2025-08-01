using System.Linq;
using System.Text.Json.Serialization;
using Content.Shared._Funkystation.Records;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Records;

/// <summary>
/// Contains Cosmatic Drift records that can be changed in the character editor. This is stored on the character's profile.
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class PlayerProvidedCharacterRecords
{
    public const int TextMedLen = 64;
    public const int TextVeryLargeLen = 4096;

    /* Basic info */

    // Additional data is fetched from the Profile

    // All
    [DataField]
    public int Height { get; private set; }

    [DataField]
    public int Weight { get; private set; }
    public const int MaxWeight = 300;

    [DataField]
    public string EmergencyContactName { get; private set; }

    // Employment
    [DataField]
    public bool HasWorkAuthorization { get; private set; }

    // Security
    [DataField]
    public string IdentifyingFeatures { get; private set; }

    // Medical
    [DataField]
    public string PostmortemInstructions { get; private set; }

    [DataField]
    public bool HasInsurance { get; private set; }

    [DataField]
    public int InsuranceProvider { get; private set; }

    [DataField]
    public int InsuranceType { get; private set; }

    // medical info

    /// <summary>
    /// A character's enabled medical info,
    /// includes: allergies, prescriptions, family history
    /// </summary>
    [DataField]
    private HashSet<ProtoId<MedicalInfoPrototype>> _medicalInfo = new();
    public HashSet<ProtoId<MedicalInfoPrototype>> MedicalInfo => _medicalInfo;

    [DataField]
    public int BloodType { get; private set; }
    // history, prescriptions, etc. would be a record below

    // "incidents"
    [DataField, JsonIgnore]
    public List<RecordEntry> MedicalEntries { get; private set; }
    [DataField, JsonIgnore]
    public List<RecordEntry> SecurityEntries { get; private set; }
    [DataField, JsonIgnore]
    public List<RecordEntry> EmploymentEntries { get; private set; }

    [DataDefinition]
    [Serializable, NetSerializable]
    public sealed partial class RecordEntry
    {
        [DataField]
        public string Title { get; private set; }
        // players involved, can be left blank (or with a generic "CentCom" etc.) for backstory related issues
        [DataField]
        public string Involved { get; private set; }
        // Longer description of events.
        [DataField]
        public string Description { get; private set; }

        public RecordEntry(string title, string involved, string desc)
        {
            Title = title;
            Involved = involved;
            Description = desc;
        }

        public RecordEntry(RecordEntry other)
        : this(other.Title, other.Involved, other.Description)
        {
        }

        public bool MemberwiseEquals(RecordEntry other)
        {
            return Title == other.Title && Involved == other.Involved && Description == other.Description;
        }

        public void EnsureValid()
        {
            Title = ClampString(Title, TextMedLen);
            Involved = ClampString(Involved, TextMedLen);
            Description = ClampString(Description, TextVeryLargeLen);
        }
    }

    public PlayerProvidedCharacterRecords(
        bool hasWorkAuthorization,
        int height,
        int weight,
        string emergencyContactName,
        string identifyingFeatures,
        bool hasInsurance,
        int insuranceProvider, int insuranceType,
        HashSet<ProtoId<MedicalInfoPrototype>> medicalInfo,
        int bloodType,
        string postmortemInstructions,
        List<RecordEntry> medicalEntries, List<RecordEntry> securityEntries, List<RecordEntry> employmentEntries)
    {
        HasWorkAuthorization = hasWorkAuthorization;
        Height = height;
        Weight = weight;
        EmergencyContactName = emergencyContactName;
        IdentifyingFeatures = identifyingFeatures;
        PostmortemInstructions = postmortemInstructions;
        HasInsurance = hasInsurance;
        InsuranceProvider = insuranceProvider;
        InsuranceType = insuranceType;
        _medicalInfo = medicalInfo;
        MedicalEntries = medicalEntries;
        SecurityEntries = securityEntries;
        EmploymentEntries = employmentEntries;
    }

    public PlayerProvidedCharacterRecords(PlayerProvidedCharacterRecords other)
    {
        Height = other.Height;
        Weight = other.Weight;
        EmergencyContactName = other.EmergencyContactName;
        HasWorkAuthorization = other.HasWorkAuthorization;
        IdentifyingFeatures = other.IdentifyingFeatures;
        HasInsurance = other.HasInsurance;
        InsuranceProvider = other.InsuranceProvider;
        InsuranceType = other.InsuranceType;
        _medicalInfo = other.MedicalInfo;
        PostmortemInstructions = other.PostmortemInstructions;
        MedicalEntries = other.MedicalEntries.Select(x => new RecordEntry(x)).ToList();
        SecurityEntries = other.SecurityEntries.Select(x => new RecordEntry(x)).ToList();
        EmploymentEntries = other.EmploymentEntries.Select(x => new RecordEntry(x)).ToList();
    }

    public static PlayerProvidedCharacterRecords DefaultRecords()
    {
        return new PlayerProvidedCharacterRecords(
            hasWorkAuthorization: true,
            height: 170, weight: 70,
            emergencyContactName: "",
            identifyingFeatures: "",
            hasInsurance: true,
            insuranceProvider: 0,
            insuranceType: 0,
            medicalInfo: [],
            bloodType: 0,
            postmortemInstructions: "Return home",
            medicalEntries: new List<RecordEntry>(),
            securityEntries: new List<RecordEntry>(),
            employmentEntries: new List<RecordEntry>()
        );
    }

    public bool MemberwiseEquals(PlayerProvidedCharacterRecords other)
    {
        // This is ugly but is only used for integration tests.
        var test = Height == other.Height
                   && Weight == other.Weight
                   && EmergencyContactName == other.EmergencyContactName
                   && HasWorkAuthorization == other.HasWorkAuthorization
                   && IdentifyingFeatures == other.IdentifyingFeatures
                   && HasInsurance == other.HasInsurance
                   && InsuranceProvider == other.InsuranceProvider
                   && InsuranceType == other.InsuranceType
                   && _medicalInfo.SetEquals(other.MedicalInfo)
                   && PostmortemInstructions == other.PostmortemInstructions;
        if (!test)
            return false;
        if (MedicalEntries.Count != other.MedicalEntries.Count)
            return false;
        if (SecurityEntries.Count != other.SecurityEntries.Count)
            return false;
        if (EmploymentEntries.Count != other.EmploymentEntries.Count)
            return false;
        if (MedicalEntries.Where((t, i) => !t.MemberwiseEquals(other.MedicalEntries[i])).Any())
        {
            return false;
        }
        if (SecurityEntries.Where((t, i) => !t.MemberwiseEquals(other.SecurityEntries[i])).Any())
        {
            return false;
        }
        if (EmploymentEntries.Where((t, i) => !t.MemberwiseEquals(other.EmploymentEntries[i])).Any())
        {
            return false;
        }

        return true;
    }

    private static string ClampString(string str, int maxLen)
    {
        if (str.Length > maxLen)
        {
            return str[..maxLen];
        }
        return str;
    }

    private static void EnsureValidEntries(List<RecordEntry> entries)
    {
        foreach (var entry in entries)
        {
            entry.EnsureValid();
        }
    }

    /// <summary>
    /// Clamp invalid entries to valid values
    /// </summary>
    public void EnsureValid()
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var info = MedicalInfo
            .Where(prototypeManager.HasIndex)
            .ToList();
        
        Weight = Math.Clamp(Weight, 0, MaxWeight);
        EmergencyContactName =
            ClampString(EmergencyContactName, TextMedLen);
        IdentifyingFeatures = ClampString(IdentifyingFeatures, TextMedLen);
        PostmortemInstructions = ClampString(PostmortemInstructions, TextMedLen);
        _medicalInfo.UnionWith(GetValidInfo(info, prototypeManager));

        EnsureValidEntries(EmploymentEntries);
        EnsureValidEntries(MedicalEntries);
        EnsureValidEntries(SecurityEntries);
    }
    public PlayerProvidedCharacterRecords WithHeight(int height)
    {
        return new(this) { Height = height };
    }
    public PlayerProvidedCharacterRecords WithWeight(int weight)
    {
        return new(this) { Weight = weight };
    }
    public PlayerProvidedCharacterRecords WithWorkAuth(bool auth)
    {
        return new(this) { HasWorkAuthorization = auth };
    }
    public PlayerProvidedCharacterRecords WithContactName(string name)
    {
        return new(this) { EmergencyContactName = name};
    }
    public PlayerProvidedCharacterRecords WithIdentifyingFeatures(string feat)
    {
        return new(this) { IdentifyingFeatures = feat};
    }

    public PlayerProvidedCharacterRecords WithInsurance(bool hasInsurance)
    {
        return new (this) { HasInsurance = hasInsurance };
    }

    public PlayerProvidedCharacterRecords WithInsuranceProvider(int insuranceProvider)
    {
        return new (this) { InsuranceProvider = insuranceProvider };
    }

    public PlayerProvidedCharacterRecords WithInsuranceType(int insuranceType)
    {
        return new (this) { InsuranceType = insuranceType };
    }

    public PlayerProvidedCharacterRecords WithMedicalInfo(ProtoId<MedicalInfoPrototype> medicalId, IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(medicalId, out var medicalInfoProto))
            return new(this);

        var category = medicalInfoProto.Category;

        MedicalInfoCategoryPrototype? categoryProto = null;

        if (category != null && !protoManager.TryIndex(category, out categoryProto))
            return new(this);

        var list = new HashSet<ProtoId<MedicalInfoPrototype>>(_medicalInfo) { medicalId };

        if (categoryProto == null)
            return new(this) {_medicalInfo = list};

        foreach (var item in list)
        {
            if (!protoManager.TryIndex(item, out var otherProto)
                || otherProto.Category != categoryProto)
                continue;
        }

        return new(this) { _medicalInfo = list };
    }

    public PlayerProvidedCharacterRecords WithoutMedicalInfo(ProtoId<MedicalInfoPrototype> medicalId, IPrototypeManager protoManager)
    {
        var list = new HashSet<ProtoId<MedicalInfoPrototype>>(_medicalInfo);
        list.Remove(medicalId);

        return new(this) { _medicalInfo = list };
    }

    public PlayerProvidedCharacterRecords WithBloodType(int b)
    {
        return new (this) { BloodType = b };
    }
    public PlayerProvidedCharacterRecords WithPostmortemInstructions(string s)
    {
        return new(this) { PostmortemInstructions = s};
    }
    public PlayerProvidedCharacterRecords WithEmploymentEntries(List<RecordEntry> entries)
    {
        return new(this) { EmploymentEntries = entries};
    }
    public PlayerProvidedCharacterRecords WithMedicalEntries(List<RecordEntry> entries)
    {
        return new(this) { MedicalEntries = entries};
    }
    public PlayerProvidedCharacterRecords WithSecurityEntries(List<RecordEntry> entries)
    {
        return new(this) { SecurityEntries = entries};
    }

    public HashSet<ProtoId<MedicalInfoPrototype>> GetValidInfo(IEnumerable<ProtoId<MedicalInfoPrototype>> info, IPrototypeManager protoManager)
    {
        var result = new HashSet<ProtoId<MedicalInfoPrototype>>();

        foreach (var item in info)
        {
            if (!protoManager.TryIndex(item, out var itemProto))
                continue;

            if (itemProto.Category == null)
                continue;

            // No category so dump it.
            if (!protoManager.TryIndex(itemProto.Category, out var category))
                continue;

            result.Add(item);
        }

        return result;
    }
}

public enum CharacterRecordType : byte
{
    Employment, Medical, Security
}
