using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Localization;
using System.Linq;

namespace SafeChests.UI
{
    internal class ChestButtonUI : UIState
    {
        private enum UIMode { None, Protect, Unlock }
        private UITextPanel<string> protectButton;
        private UITextPanel<string> passwordLabel;
        private UIInputTextField passwordInput;
        private UITextPanel<string> confirmPasswordLabel;
        private UIInputTextField confirmPasswordInput;
        private UITextPanel<string> unlockPasswordLabel;
        private UIInputTextField unlockPasswordInput;
        private UITextPanel<string> acceptButton;
        private string lastPasswordInput = "";
        private string lastConfirmPasswordInput = "";
        private string lastUnlockPasswordInput = "";
        private UIMode currentMode = UIMode.None;
        private int lastChestIndex = -1;

        // Variables para detectar cambio de idioma
        private string lastLanguage = "";
        private bool needsLanguageUpdate = false;

        // Método mejorado para obtener texto localizado
        private string GetLocalizedText(string key)
        {
            try
            {
                string text = Language.GetTextValue(key);

                // Si el texto devuelto es igual a la clave, usar fallbacks según el idioma
                if (text == key)
                {
                    return GetFallbackText(key);
                }

                return text;
            }
            catch
            {
                return GetFallbackText(key);
            }
        }

        private string GetFallbackText(string key)
        {
            // Detectar si el idioma actual es español usando el nombre de la cultura
            string cultureName = Language.ActiveCulture.Name;
            bool isSpanish = cultureName.StartsWith("es") || cultureName.Contains("Spanish");

            return key switch
            {
                "Mods.SafeChests.UI.ProtectChest" => isSpanish ? "Proteger cofre" : "Protect Chest",
                "Mods.SafeChests.UI.UnlockChest" => isSpanish ? "Desbloquear cofre" : "Unlock Chest",
                "Mods.SafeChests.UI.Password" => isSpanish ? "Contraseña:" : "Password:",
                "Mods.SafeChests.UI.Confirm" => isSpanish ? "Confirmar:" : "Confirm:",
                "Mods.SafeChests.UI.EnterPassword" => isSpanish ? "Ingresar contraseña" : "Enter Password",
                "Mods.SafeChests.UI.Accept" => isSpanish ? "Aceptar" : "Accept",
                "Mods.SafeChests.UI.PasswordPlaceholder" => isSpanish ? "Escribe la contraseña" : "Enter password",
                "Mods.SafeChests.UI.ConfirmPlaceholder" => isSpanish ? "Confirma la contraseña" : "Confirm password",
                "Mods.SafeChests.UI.UnlockPlaceholder" => isSpanish ? "Ingresa la contraseña" : "Enter password",
                "Mods.SafeChests.Messages.OnlyWorldChests" => isSpanish ? "Solo se pueden proteger cofres del mundo." : "Only world chests can be protected.",
                "Mods.SafeChests.Messages.NoChestOpen" => isSpanish ? "No hay ningún cofre abierto." : "No chest is open.",
                "Mods.SafeChests.Messages.EmptyPassword" => isSpanish ? "La contraseña no puede estar vacía." : "Password cannot be empty.",
                "Mods.SafeChests.Messages.PasswordMismatch" => isSpanish ? "Las contraseñas no coinciden." : "Passwords do not match.",
                "Mods.SafeChests.Messages.IncorrectPassword" => isSpanish ? "Contraseña incorrecta." : "Incorrect password.",
                "Mods.SafeChests.Messages.UIInitialized" => isSpanish ? "SafeChests: UI inicializada" : "SafeChests: UI initialized",
                _ => key
            };
        }

        private bool IsRealChest()
        {
            Player player = Main.player[Main.myPlayer];

            if (player.chest == -1) return false;
            if (player.chest == -2) return false; // Hucha
            if (player.chest == -3) return false; // Caja fuerte
            if (player.chest == -4) return false; // Cofre del Vacío
            if (player.chest == -5) return false; // Cofre de Equipo

            Chest chest = Main.chest[player.chest];
            return chest != null;
        }

        // Método para recrear los inputs con los nuevos placeholders
        private void RecreateInputs()
        {
            // Guardar el texto actual de los inputs
            string currentPasswordText = passwordInput?.Text ?? "";
            string currentConfirmPasswordText = confirmPasswordInput?.Text ?? "";
            string currentUnlockPasswordText = unlockPasswordInput?.Text ?? "";

            // Recrear passwordInput
            if (passwordInput != null)
            {
                passwordInput.Remove();
            }
            passwordInput = new UIInputTextField("", GetLocalizedText("Mods.SafeChests.UI.PasswordPlaceholder"))
            {
                Width = { Pixels = 172 },
                Height = { Pixels = 35 },
                Left = { Pixels = 180 },
                Top = { Pixels = 472 }
            };
            passwordInput.Text = currentPasswordText;

            // Recrear confirmPasswordInput
            if (confirmPasswordInput != null)
            {
                confirmPasswordInput.Remove();
            }
            confirmPasswordInput = new UIInputTextField("", GetLocalizedText("Mods.SafeChests.UI.ConfirmPlaceholder"))
            {
                Width = { Pixels = 172 },
                Height = { Pixels = 35 },
                Left = { Pixels = 180 },
                Top = { Pixels = 516 }
            };
            confirmPasswordInput.Text = currentConfirmPasswordText;

            // Recrear unlockPasswordInput
            if (unlockPasswordInput != null)
            {
                unlockPasswordInput.Remove();
            }
            unlockPasswordInput = new UIInputTextField("", GetLocalizedText("Mods.SafeChests.UI.UnlockPlaceholder"))
            {
                Width = { Pixels = 158 },
                Height = { Pixels = 35 },
                Left = { Pixels = 225 },
                Top = { Pixels = 302 }
            };
            unlockPasswordInput.Text = currentUnlockPasswordText;
        }

        // Método para actualizar todos los textos cuando cambia el idioma
        private void UpdateAllTexts()
        {
            // Actualizar el botón principal
            Player player = Main.player[Main.myPlayer];
            if (player.chest != -1 && IsRealChest())
            {
                Chest chest = Main.chest[player.chest];
                if (chest != null)
                {
                    bool isLocked = ChestProtectionSystem.IsChestProtected(chest.x, chest.y);
                    string buttonText = isLocked ?
                        GetLocalizedText("Mods.SafeChests.UI.UnlockChest") :
                        GetLocalizedText("Mods.SafeChests.UI.ProtectChest");
                    protectButton.SetText(buttonText);
                }
            }
            else
            {
                protectButton.SetText(GetLocalizedText("Mods.SafeChests.UI.ProtectChest"));
            }

            // Actualizar labels
            passwordLabel?.SetText(GetLocalizedText("Mods.SafeChests.UI.Password"));
            confirmPasswordLabel?.SetText(GetLocalizedText("Mods.SafeChests.UI.Confirm"));
            unlockPasswordLabel?.SetText(GetLocalizedText("Mods.SafeChests.UI.EnterPassword"));
            acceptButton?.SetText(GetLocalizedText("Mods.SafeChests.UI.Accept"));

            // Recrear los inputs con los nuevos placeholders
            RecreateInputs();

            // Actualizar los elementos de la UI
            UpdateUIElements();
        }

        public override void OnInitialize()
        {
            lastLanguage = Language.ActiveCulture.Name;

            protectButton = new UITextPanel<string>(GetLocalizedText("Mods.SafeChests.UI.ProtectChest"), 0.8f, false)
            {
                Width = { Pixels = 120 },
                Height = { Pixels = 30 },
                Left = { Pixels = 73 },
                Top = { Pixels = 428 }
            };

            protectButton.OnMouseOver += (evt, element) =>
                protectButton.BorderColor = Color.Yellow;

            protectButton.OnMouseOut += (evt, element) =>
                protectButton.BorderColor = Color.Black;

            protectButton.OnLeftClick += (evt, element) =>
            {
                Player player = Main.player[Main.myPlayer];
                int chestIndex = player.chest;

                if (chestIndex != -1 && IsRealChest())
                {
                    Chest chest = Main.chest[chestIndex];
                    if (chest != null)
                    {
                        bool isLocked = ChestProtectionSystem.IsChestProtected(chest.x, chest.y);
                        currentMode = isLocked ? UIMode.Unlock : UIMode.Protect;
                        string buttonText = isLocked ?
                            GetLocalizedText("Mods.SafeChests.UI.UnlockChest") :
                            GetLocalizedText("Mods.SafeChests.UI.ProtectChest");
                        protectButton.SetText(buttonText);
                        protectButton.Top.Pixels = isLocked ? 258 : 428;
                    }
                }
                else if (!IsRealChest())
                {
                    ChestProtectionSystem.SendMessageToAll(
                        GetLocalizedText("Mods.SafeChests.Messages.OnlyWorldChests"),
                        Color.Red);
                    currentMode = UIMode.None;
                }
                else
                {
                    ChestProtectionSystem.SendMessageToAll(
                        GetLocalizedText("Mods.SafeChests.Messages.NoChestOpen"),
                        Color.Red);
                    currentMode = UIMode.None;
                }

                UpdateUIElements();
            };

            passwordLabel = new UITextPanel<string>(GetLocalizedText("Mods.SafeChests.UI.Password"), 0.8f, false)
            {
                Width = { Pixels = 80 },
                Height = { Pixels = 30 },
                Left = { Pixels = 73 },
                Top = { Pixels = 472 }
            };

            passwordInput = new UIInputTextField("", GetLocalizedText("Mods.SafeChests.UI.PasswordPlaceholder"))
            {
                Width = { Pixels = 172 },
                Height = { Pixels = 35 },
                Left = { Pixels = 180 },
                Top = { Pixels = 472 }
            };

            confirmPasswordLabel = new UITextPanel<string>(GetLocalizedText("Mods.SafeChests.UI.Confirm"), 0.8f, false)
            {
                Width = { Pixels = 97 },
                Height = { Pixels = 30 },
                Left = { Pixels = 73 },
                Top = { Pixels = 516 }
            };

            confirmPasswordInput = new UIInputTextField("", GetLocalizedText("Mods.SafeChests.UI.ConfirmPlaceholder"))
            {
                Width = { Pixels = 172 },
                Height = { Pixels = 35 },
                Left = { Pixels = 180 },
                Top = { Pixels = 516 }
            };

            unlockPasswordLabel = new UITextPanel<string>(GetLocalizedText("Mods.SafeChests.UI.EnterPassword"), 0.8f, false)
            {
                Width = { Pixels = 120 },
                Height = { Pixels = 30 },
                Left = { Pixels = 73 },
                Top = { Pixels = 302 }
            };

            unlockPasswordInput = new UIInputTextField("", GetLocalizedText("Mods.SafeChests.UI.UnlockPlaceholder"))
            {
                Width = { Pixels = 158 },
                Height = { Pixels = 35 },
                Left = { Pixels = 225 },
                Top = { Pixels = 302 }
            };

            acceptButton = new UITextPanel<string>(GetLocalizedText("Mods.SafeChests.UI.Accept"), 0.8f, false)
            {
                Width = { Pixels = 80 },
                Height = { Pixels = 30 },
                Left = { Pixels = 415 },
                Top = { Pixels = 470 }
            };

            acceptButton.OnMouseOver += (evt, element) =>
                acceptButton.BorderColor = Color.Yellow;

            acceptButton.OnMouseOut += (evt, element) =>
                acceptButton.BorderColor = Color.Black;

            acceptButton.OnLeftClick += (evt, element) =>
            {
                Player player = Main.player[Main.myPlayer];
                int chestIndex = player.chest;

                if (chestIndex != -1 && IsRealChest())
                {
                    Chest chest = Main.chest[chestIndex];
                    if (chest != null)
                    {
                        if (currentMode == UIMode.Protect)
                        {
                            if (passwordInput.Text == confirmPasswordInput.Text)
                            {
                                if (!string.IsNullOrEmpty(passwordInput.Text))
                                {
                                    if (Main.netMode == NetmodeID.MultiplayerClient)
                                    {
                                        ModPacket packet = ModContent.GetInstance<SafeChests>().GetPacket();
                                        packet.Write((byte)0);
                                        packet.Write(chest.x);
                                        packet.Write(chest.y);
                                        packet.Write(passwordInput.Text);
                                        packet.Write(false);
                                        packet.Send();
                                        protectButton.SetText(GetLocalizedText("Mods.SafeChests.UI.UnlockChest"));
                                        protectButton.Top.Pixels = 258;
                                    }
                                    else
                                    {
                                        ChestProtectionSystem.ToggleChestProtection(chest.x, chest.y, passwordInput.Text);
                                        protectButton.SetText(GetLocalizedText("Mods.SafeChests.UI.UnlockChest"));
                                        protectButton.Top.Pixels = 258;
                                    }
                                    passwordInput.Text = "";
                                    confirmPasswordInput.Text = "";
                                    currentMode = UIMode.None;
                                }
                                else
                                {
                                    ChestProtectionSystem.SendMessageToAll(
                                        GetLocalizedText("Mods.SafeChests.Messages.EmptyPassword"),
                                        Color.Red);
                                }
                            }
                            else
                            {
                                ChestProtectionSystem.SendMessageToAll(
                                    GetLocalizedText("Mods.SafeChests.Messages.PasswordMismatch"),
                                    Color.Red);
                            }
                        }
                        else if (currentMode == UIMode.Unlock)
                        {
                            string storedPassword = ChestProtectionSystem.GetChestPassword(chest.x, chest.y);
                            if (unlockPasswordInput.Text == storedPassword)
                            {
                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                {
                                    ModPacket packet = ModContent.GetInstance<SafeChests>().GetPacket();
                                    packet.Write((byte)0);
                                    packet.Write(chest.x);
                                    packet.Write(chest.y);
                                    packet.Write("");
                                    packet.Write(true);
                                    packet.Send();
                                    protectButton.SetText(GetLocalizedText("Mods.SafeChests.UI.ProtectChest"));
                                    protectButton.Top.Pixels = 428;
                                }
                                else
                                {
                                    ChestProtectionSystem.ToggleChestProtection(chest.x, chest.y, "");
                                    protectButton.SetText(GetLocalizedText("Mods.SafeChests.UI.ProtectChest"));
                                    protectButton.Top.Pixels = 428;
                                }
                                unlockPasswordInput.Text = "";
                                currentMode = UIMode.None;
                            }
                            else
                            {
                                ChestProtectionSystem.SendMessageToAll(
                                    GetLocalizedText("Mods.SafeChests.Messages.IncorrectPassword"),
                                    Color.Red);
                            }
                        }
                        UpdateUIElements();
                    }
                }
                else if (!IsRealChest())
                {
                    ChestProtectionSystem.SendMessageToAll(
                        GetLocalizedText("Mods.SafeChests.Messages.OnlyWorldChests"),
                        Color.Red);
                    currentMode = UIMode.None;
                    UpdateUIElements();
                }
                else
                {
                    ChestProtectionSystem.SendMessageToAll(
                        GetLocalizedText("Mods.SafeChests.Messages.NoChestOpen"),
                        Color.Red);
                    currentMode = UIMode.None;
                    UpdateUIElements();
                }
            };

            Append(protectButton);
            UpdateUIElements();
        }

        private void UpdateUIElements()
        {
            passwordLabel.Remove();
            passwordInput.Remove();
            confirmPasswordLabel.Remove();
            confirmPasswordInput.Remove();
            unlockPasswordLabel.Remove();
            unlockPasswordInput.Remove();
            acceptButton.Remove();

            if (currentMode == UIMode.Protect)
            {
                if (!Children.Contains(passwordLabel)) Append(passwordLabel);
                if (!Children.Contains(passwordInput)) Append(passwordInput);
                if (!Children.Contains(confirmPasswordLabel)) Append(confirmPasswordLabel);
                if (!Children.Contains(confirmPasswordInput)) Append(confirmPasswordInput);
                if (!Children.Contains(acceptButton)) Append(acceptButton);
                acceptButton.Top.Pixels = 470;
            }
            else if (currentMode == UIMode.Unlock)
            {
                if (!Children.Contains(unlockPasswordLabel)) Append(unlockPasswordLabel);
                if (!Children.Contains(unlockPasswordInput)) Append(unlockPasswordInput);
                if (!Children.Contains(acceptButton)) Append(acceptButton);
                acceptButton.Top.Pixels = 300;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Detectar cambio de idioma
            string currentLanguage = Language.ActiveCulture.Name;
            if (currentLanguage != lastLanguage)
            {
                lastLanguage = currentLanguage;
                needsLanguageUpdate = true;
            }

            // Actualizar textos si cambió el idioma
            if (needsLanguageUpdate)
            {
                UpdateAllTexts();
                needsLanguageUpdate = false;
            }

            if (!IsRealChest())
            {
                return;
            }

            if (currentMode == UIMode.Protect)
            {
                if (passwordInput.Text != lastPasswordInput)
                {
                    lastPasswordInput = passwordInput.Text;
                }
                if (confirmPasswordInput.Text != lastConfirmPasswordInput)
                {
                    lastConfirmPasswordInput = confirmPasswordInput.Text;
                }
            }
            else if (currentMode == UIMode.Unlock)
            {
                if (unlockPasswordInput.Text != lastUnlockPasswordInput)
                {
                    lastUnlockPasswordInput = unlockPasswordInput.Text;
                }
            }

            Player player = Main.player[Main.myPlayer];
            if (player.chest != lastChestIndex)
            {
                passwordInput.Text = "";
                confirmPasswordInput.Text = "";
                unlockPasswordInput.Text = "";
                lastPasswordInput = "";
                lastConfirmPasswordInput = "";
                lastUnlockPasswordInput = "";

                lastChestIndex = player.chest;
                if (player.chest != -1 && IsRealChest())
                {
                    Chest chest = Main.chest[player.chest];
                    if (chest != null)
                    {
                        bool isLocked = ChestProtectionSystem.IsChestProtected(chest.x, chest.y);
                        string buttonText = isLocked ?
                            GetLocalizedText("Mods.SafeChests.UI.UnlockChest") :
                            GetLocalizedText("Mods.SafeChests.UI.ProtectChest");
                        protectButton.SetText(buttonText);
                        protectButton.Top.Pixels = isLocked ? 258 : 428;
                        currentMode = UIMode.None;
                    }
                }
                else
                {
                    currentMode = UIMode.None;
                }
                UpdateUIElements();
            }
        }

        public override void OnActivate()
        {
            Player player = Main.player[Main.myPlayer];
            lastChestIndex = player.chest;
            if (player.chest != -1 && IsRealChest())
            {
                Chest chest = Main.chest[player.chest];
                if (chest != null)
                {
                    bool isLocked = ChestProtectionSystem.IsChestProtected(chest.x, chest.y);
                    string buttonText = isLocked ?
                        GetLocalizedText("Mods.SafeChests.UI.UnlockChest") :
                        GetLocalizedText("Mods.SafeChests.UI.ProtectChest");
                    protectButton.SetText(buttonText);
                    protectButton.Top.Pixels = isLocked ? 258 : 428;
                    currentMode = UIMode.None;
                    UpdateUIElements();
                }
            }
            else
            {
                passwordInput.Text = "";
                confirmPasswordInput.Text = "";
                unlockPasswordInput.Text = "";
                lastPasswordInput = "";
                lastConfirmPasswordInput = "";
                lastUnlockPasswordInput = "";
                currentMode = UIMode.None;
                UpdateUIElements();
            }

            // Actualizar textos al activar la UI
            UpdateAllTexts();
        }

        public void UpdateButtonText(string text)
        {
            protectButton?.SetText(text);
            string unlockText = GetLocalizedText("Mods.SafeChests.UI.UnlockChest");
            bool isUnlock = text == unlockText;
            protectButton.Top.Pixels = isUnlock ? 258 : 428;
            currentMode = UIMode.None;
            UpdateUIElements();
        }
    }
}
