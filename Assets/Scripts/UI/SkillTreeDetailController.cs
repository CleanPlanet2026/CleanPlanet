using System;
using CleanPlanet.Core.Currency;
using CleanPlanet.Upgrade;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    public sealed class SkillTreeDetailController : MonoBehaviour
    {
        private enum UpgradeStatus
        {
            Locked,
            Available,
            Completed
        }

        [Serializable]
        private sealed class SkillOption
        {
            [SerializeField] private Button _button;
            [SerializeField] private string _upgradeId;
            [SerializeField] private string _prerequisiteId;
            [SerializeField, Min(0)] private int _initialLevel;
            [SerializeField, Min(1)] private int _maxLevel = 1;
            [SerializeField] private string _state;
            [SerializeField] private string _skillName;
            [SerializeField] private string _branch;
            [SerializeField, TextArea] private string _description;
            [SerializeField] private string _currentEffect;
            [SerializeField] private string _nextEffect;
            [SerializeField] private string _cost;
            [SerializeField, Min(0)] private int _costAmount;

            public Button Button => _button;
            public string UpgradeId => _upgradeId;
            public string PrerequisiteId => _prerequisiteId;
            public int InitialLevel => _initialLevel;
            public int MaxLevel => _maxLevel;
            public string State => _state;
            public string SkillName => _skillName;
            public string Branch => _branch;
            public string Description => _description;
            public string CurrentEffect => _currentEffect;
            public string NextEffect => _nextEffect;
            public string Cost => _cost;
            public int CostAmount => _costAmount;
        }

        [Serializable]
        private sealed class SkillConnection
        {
            [SerializeField] private string _fromUpgradeId;
            [SerializeField] private string _toUpgradeId;
            [SerializeField] private Graphic _line;

            public string FromUpgradeId => _fromUpgradeId;
            public string ToUpgradeId => _toUpgradeId;
            public Graphic Line => _line;
        }

        [SerializeField] private Text _stateText;
        [SerializeField] private Text _skillNameText;
        [SerializeField] private Text _branchText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Text _currentEffectText;
        [SerializeField] private Text _nextEffectText;
        [SerializeField] private Text _costText;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private CurrencyWallet _wallet;
        [SerializeField] private SkillOption[] _options;
        [SerializeField] private SkillConnection[] _connections;
        [SerializeField, Min(0)] private int _defaultOptionIndex;
        [SerializeField] private Color _lockedColor = new(0.25f, 0.32f, 0.35f, 1f);
        [SerializeField] private Color _availableColor = new(1f, 0.82f, 0.4f, 1f);
        [SerializeField] private Color _upgradedColor = new(0.22f, 0.85f, 0.77f, 1f);

        private readonly UpgradeRuntimeState _runtimeState = UpgradeRuntimeState.Shared;
        private UnityAction[] _selectionActions;
        private int _selectedOptionIndex;

        private void OnEnable()
        {
            if (!HasRequiredReferences())
            {
                Debug.LogError($"{nameof(SkillTreeDetailController)} requires all text and option references.", this);
                enabled = false;
                return;
            }

            _wallet.GoldChanged += HandleGoldChanged;
            _upgradeButton.onClick.AddListener(UpgradeSelectedOption);
            _selectionActions = new UnityAction[_options.Length];
            for (int i = 0; i < _options.Length; i++)
            {
                int optionIndex = i;
                _selectionActions[i] = () => SelectOption(optionIndex);
                _options[i].Button.onClick.AddListener(_selectionActions[i]);
            }

            RefreshTreeVisuals();
            SelectOption(Mathf.Clamp(_defaultOptionIndex, 0, _options.Length - 1));
        }

        private void OnDisable()
        {
            if (_wallet != null)
            {
                _wallet.GoldChanged -= HandleGoldChanged;
            }

            if (_upgradeButton != null)
            {
                _upgradeButton.onClick.RemoveListener(UpgradeSelectedOption);
            }

            if (_selectionActions == null || _options == null)
            {
                return;
            }

            int count = Mathf.Min(_selectionActions.Length, _options.Length);
            for (int i = 0; i < count; i++)
            {
                if (_options[i]?.Button != null)
                {
                    _options[i].Button.onClick.RemoveListener(_selectionActions[i]);
                }
            }

            _selectionActions = null;
        }

        private void SelectOption(int optionIndex)
        {
            _selectedOptionIndex = optionIndex;
            SkillOption option = _options[optionIndex];
            _skillNameText.text = option.SkillName;
            _branchText.text = option.Branch;
            _descriptionText.text = option.Description;
            RefreshUpgradeState(option);
            option.Button.Select();
        }

        private void UpgradeSelectedOption()
        {
            SkillOption option = _options[_selectedOptionIndex];
            if (GetStatus(option) != UpgradeStatus.Available || !_wallet.TrySpend(option.CostAmount))
            {
                RefreshUpgradeState(option);
                return;
            }

            if (_runtimeState.TryUpgrade(option.UpgradeId, option.InitialLevel, option.MaxLevel))
            {
                RefreshTreeVisuals();
                RefreshUpgradeState(option);
            }
        }

        private void HandleGoldChanged(int _)
        {
            RefreshUpgradeState(_options[_selectedOptionIndex]);
        }

        private void RefreshUpgradeState(SkillOption option)
        {
            int level = _runtimeState.GetLevel(option.UpgradeId, option.InitialLevel);
            UpgradeStatus status = GetStatus(option);
            bool isCompleted = status == UpgradeStatus.Completed;

            string state = status switch
            {
                UpgradeStatus.Completed when option.InitialLevel < option.MaxLevel => "■ 강화 완료",
                UpgradeStatus.Completed => option.State,
                UpgradeStatus.Available => "◆ 구매 가능",
                _ => "□ 잠김"
            };
            _stateText.text = $"{state}  Lv. {level}/{option.MaxLevel}";
            _currentEffectText.text = level > option.InitialLevel
                ? option.NextEffect
                : option.CurrentEffect;
            _nextEffectText.text = isCompleted ? "최대 레벨" : option.NextEffect;
            _costText.text = status switch
            {
                UpgradeStatus.Completed => "-",
                UpgradeStatus.Locked => "선행 강화 필요",
                _ => option.Cost
            };
            _upgradeButton.interactable = status == UpgradeStatus.Available
                && _wallet.Gold >= option.CostAmount;
            UpdateNodeVisual(option, status);
        }

        private void RefreshTreeVisuals()
        {
            foreach (SkillOption option in _options)
            {
                UpdateNodeVisual(option, GetStatus(option));
            }

            foreach (SkillConnection connection in _connections)
            {
                SkillOption from = GetOption(connection.FromUpgradeId);
                SkillOption to = GetOption(connection.ToUpgradeId);
                UpgradeStatus status = GetStatus(to);
                connection.Line.color = status == UpgradeStatus.Completed
                    ? _upgradedColor
                    : GetStatus(from) == UpgradeStatus.Completed
                        ? _availableColor
                        : _lockedColor;
            }
        }

        private void UpdateNodeVisual(SkillOption option, UpgradeStatus status)
        {
            Color normalColor = status switch
            {
                UpgradeStatus.Completed => _upgradedColor,
                UpgradeStatus.Available => _availableColor,
                _ => _lockedColor
            };
            ColorBlock colors = option.Button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.2f);
            colors.selectedColor = normalColor;
            option.Button.colors = colors;
            option.Button.targetGraphic.color = normalColor;
        }

        private UpgradeStatus GetStatus(SkillOption option)
        {
            int level = _runtimeState.GetLevel(option.UpgradeId, option.InitialLevel);
            if (level >= option.MaxLevel)
            {
                return UpgradeStatus.Completed;
            }

            if (string.IsNullOrEmpty(option.PrerequisiteId))
            {
                return UpgradeStatus.Available;
            }

            SkillOption prerequisite = GetOption(option.PrerequisiteId);
            int prerequisiteLevel = _runtimeState.GetLevel(
                prerequisite.UpgradeId,
                prerequisite.InitialLevel);
            return prerequisiteLevel >= prerequisite.MaxLevel
                ? UpgradeStatus.Available
                : UpgradeStatus.Locked;
        }

        private SkillOption GetOption(string upgradeId)
        {
            foreach (SkillOption option in _options)
            {
                if (option != null && option.UpgradeId == upgradeId)
                {
                    return option;
                }
            }

            return null;
        }

        private bool HasRequiredReferences()
        {
            if (_stateText == null ||
                _skillNameText == null ||
                _branchText == null ||
                _descriptionText == null ||
                _currentEffectText == null ||
                _nextEffectText == null ||
                _costText == null ||
                _upgradeButton == null ||
                _wallet == null ||
                _options == null ||
                _options.Length == 0 ||
                _connections == null)
            {
                return false;
            }

            foreach (SkillOption option in _options)
            {
                if (option == null ||
                    option.Button == null ||
                    option.Button.targetGraphic == null ||
                    string.IsNullOrWhiteSpace(option.UpgradeId) ||
                    option.InitialLevel > option.MaxLevel)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(option.PrerequisiteId)
                    && GetOption(option.PrerequisiteId) == null)
                {
                    return false;
                }
            }

            foreach (SkillConnection connection in _connections)
            {
                if (connection == null ||
                    connection.Line == null ||
                    GetOption(connection.FromUpgradeId) == null ||
                    GetOption(connection.ToUpgradeId) == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
