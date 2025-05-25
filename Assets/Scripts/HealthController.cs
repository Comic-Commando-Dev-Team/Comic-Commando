using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 100;

    [Header("Events")]
    [SerializeField] private UnityEvent<int> onHealthChanged = new();
    [SerializeField] private UnityEvent onHeal = new();
    [SerializeField] private UnityEvent onDamage = new();
    [SerializeField] private UnityEvent onDeath = new();

    private int _currentHealth;

    public int CurrentHealth => _currentHealth;
    public bool IsDead => _currentHealth <= 0;
    public UnityEvent<int> OnHealthChanged => onHealthChanged;
    public UnityEvent OnHeal => onHeal;
    public UnityEvent OnDamage => onDamage;
    public UnityEvent OnDeath => onDeath;

    private void Start()
    {
        _currentHealth = _maxHealth;
        onHealthChanged.Invoke(_currentHealth);
    }

    public void ChangeHealth(int amount)
    {
        switch (amount)
        {
            case int n when n > 0:
                Heal(amount);
                break;
            case int n when n < 0:
                Damage(-amount);
                break;
        }
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        onHealthChanged.Invoke(_currentHealth);
        onHeal.Invoke();
    }

    public void Damage(int amount)
    {
        // Prevent firing multiple death events
        if (IsDead) return;

        _currentHealth = Mathf.Max(_currentHealth - amount, 0);
        onHealthChanged.Invoke(_currentHealth);
        onDamage.Invoke();

        if (_currentHealth == 0)
        {
            onDeath.Invoke();
        }
    }
}
