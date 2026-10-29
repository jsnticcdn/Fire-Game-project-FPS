using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace AZE.AdvancedFirstPerson.Studios.Main.Scripts.Shared
{
    public class Health : MonoBehaviour
    {
        [FormerlySerializedAs("MaxHealth")]
        [Tooltip("Maximum amount of health")] public float maxHealth = 10f;

        [FormerlySerializedAs("CriticalHealthRatio")]
        [Tooltip("Health ratio at which the critical health vignette starts appearing")]
        public float criticalHealthRatio = 0.3f;

        public float Alt_Health = 200;
        UnityAction<float, GameObject> OnDamaged;
        public UnityAction<float> OnHealed;
        public UnityAction OnDie;

        public float CurrentHealth { get; set; }
        public bool Invincible { get; set; }
        public bool CanPickup() => CurrentHealth < maxHealth;

        public float GetRatio() => CurrentHealth / maxHealth;
        public bool IsCritical() => GetRatio() <= criticalHealthRatio;

        bool m_IsDead;

        public Health(UnityAction<float, GameObject> onDamaged, UnityAction<float> onHealed)
        {
            OnDamaged = onDamaged;
            OnHealed = onHealed;
        }

        void Start()
        {
            CurrentHealth = maxHealth;
        }

        public void Heal(float healAmount)
        {
            float healthBefore = CurrentHealth;
            CurrentHealth += healAmount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);

            // call OnHeal action
            float trueHealAmount = CurrentHealth - healthBefore;
            if (trueHealAmount > 0f)
            {
                OnHealed?.Invoke(trueHealAmount);
            }
        }

        public void TakeDamage(float damage, GameObject damageSource)
        {
            if (Invincible)
                return;

            float healthBefore = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);

            // call OnDamage action
            float trueDamageAmount = healthBefore - CurrentHealth;
            if (trueDamageAmount > 0f)
            {
                OnDamaged?.Invoke(trueDamageAmount, damageSource);
            }

            HandleDeath();
        }

        public void Kill()
        {
            CurrentHealth = 0f;

            // call OnDamage action
            OnDamaged?.Invoke(maxHealth, null);

            HandleDeath();
        }

        void HandleDeath()
        {
            if (m_IsDead)
                return;

            // call OnDie action
            if (CurrentHealth <= 0f)
            {
                m_IsDead = true;
                OnDie?.Invoke();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bullet"))
            {
                Alt_Health -= 5;
                Destroy(other.gameObject);
                if (Alt_Health <= 0f)
                {
                    Kill();
                    Destroy(gameObject);
                    Destroy(other.gameObject);
                }
                
            }
        }
    }
}