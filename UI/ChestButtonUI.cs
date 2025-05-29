using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using System.Linq;

namespace SafeChests.UI
{
    internal class ChestButtonUI : UIState
    {
        private enum UIMode { None, Protect, Unlock }
        private UITextPanel<string> protectButton;
        private UITextPanel<string> passwordLabel; // Etiqueta para Contraseña (Protect mode)
        private UIInputTextField passwordInput; // Input de contraseña (Protect mode)
        private UITextPanel<string> confirmPasswordLabel; // Etiqueta para Confirmar Contraseña (Protect mode)
        private UIInputTextField confirmPasswordInput; // Input de confirmar contraseña (Protect mode)
        private UITextPanel<string> unlockPasswordLabel; // Etiqueta para Ingresar contraseña (Unlock mode)
        private UIInputTextField unlockPasswordInput; // Input para desbloquear (Unlock mode)
        private UITextPanel<string> acceptButton; // Botón de Aceptar (used in both modes)
        private string lastPasswordInput = ""; // Para rastrear cambios en la contraseña (Protect mode)
        private string lastConfirmPasswordInput = ""; // Para rastrear cambios en confirmar contraseña (Protect mode)
        private string lastUnlockPasswordInput = ""; // Para rastrear cambios en la contraseña de desbloqueo (Unlock mode)
        private UIMode currentMode = UIMode.None; // Modo actual de la UI
        private int lastChestIndex = -1; // Seguimiento del último cofre abierto para detectar cambios

        public override void OnInitialize()
        {
            // Asegurarse de que el texto inicial no sea null
            protectButton = new UITextPanel<string>("Proteger cofre", 0.8f, false)
            {
                Width = { Pixels = 120 },
                Height = { Pixels = 30 },
                Left = { Pixels = 73 },
                Top = { Pixels = 428 } // Posición original para "Proteger cofre"
            };

            protectButton.OnMouseOver += (evt, element) =>
                protectButton.BorderColor = Color.Yellow;

            protectButton.OnMouseOut += (evt, element) =>
                protectButton.BorderColor = Color.Black;

            protectButton.OnLeftClick += (evt, element) =>
            {
                Player player = Main.player[Main.myPlayer];
                int chestIndex = player.chest;

                if (chestIndex != -1)
                {
                    Chest chest = Main.chest[chestIndex];
                    if (chest != null)
                    {
                        bool isLocked = ChestProtectionSystem.IsChestProtected(chest.x, chest.y);
                        currentMode = isLocked ? UIMode.Unlock : UIMode.Protect;
                        protectButton.SetText(isLocked ? "Desbloquear cofre" : "Proteger cofre");
                        protectButton.Top.Pixels = isLocked ? 258 : 428; // 528 para "Desbloquear", 428 para "Proteger"
                    }
                }
                else
                {
                    ChestProtectionSystem.SendMessageToAll("No hay ningún cofre abierto.", Color.Red);
                    currentMode = UIMode.None;
                }

                UpdateUIElements(); // Actualizar la UI del usuario dependiendo del modo
            };

            // Etiqueta "Contraseña:" (Protect mode)
            passwordLabel = new UITextPanel<string>("Contraseña:", 0.8f, false)
            {
                Width = { Pixels = 80 },
                Height = { Pixels = 30 },
                Left = { Pixels = 73 },
                Top = { Pixels = 472 } // Posición original
            };

            // Input de contraseña (Protect mode)
            passwordInput = new UIInputTextField("", "Escribe la contraseña")
            {
                Width = { Pixels = 172 },
                Height = { Pixels = 35 },
                Left = { Pixels = 180 },
                Top = { Pixels = 472 } // Posición original
            };

            // Etiqueta "Confirmar:" (Protect mode)
            confirmPasswordLabel = new UITextPanel<string>("Confirmar:", 0.8f, false)
            {
                Width = { Pixels = 97 },
                Height = { Pixels = 30 },
                Left = { Pixels = 73 },
                Top = { Pixels = 516 } // Posición original
            };

            // Input de confirmar contraseña (Protect mode)
            confirmPasswordInput = new UIInputTextField("", "Confirma la contraseña")
            {
                Width = { Pixels = 172 },
                Height = { Pixels = 35 },
                Left = { Pixels = 180 },
                Top = { Pixels = 516 } // Posición original
            };

            // Etiqueta "Ingresar contraseña" (Unlock mode)
            unlockPasswordLabel = new UITextPanel<string>("Ingresar contraseña", 0.8f, false)
            {
                Width = { Pixels = 120 },
                Height = { Pixels = 30 },
                Left = { Pixels = 73 },
                Top = { Pixels = 302 } // Modo de desbloqueo
            };

            // Input para desbloquear (Unlock mode)
            unlockPasswordInput = new UIInputTextField("", "Ingresa la contraseña")
            {
                Width = { Pixels = 158 },
                Height = { Pixels = 35 },
                Left = { Pixels = 225 },
                Top = { Pixels = 302 } // Modo de desbloqueo
            };

            // Botón de Aceptar
            acceptButton = new UITextPanel<string>("Aceptar", 0.8f, false)
            {
                Width = { Pixels = 80 },
                Height = { Pixels = 30 },
                Left = { Pixels = 415 },
                Top = { Pixels = 470 } // Posición inicial (se ajusta dinámicamente en UpdateUIElements)
            };

            acceptButton.OnMouseOver += (evt, element) =>
                acceptButton.BorderColor = Color.Yellow;

            acceptButton.OnMouseOut += (evt, element) =>
                acceptButton.BorderColor = Color.Black;

            acceptButton.OnLeftClick += (evt, element) =>
            {
                Player player = Main.player[Main.myPlayer];
                int chestIndex = player.chest;

                if (chestIndex != -1)
                {
                    Chest chest = Main.chest[chestIndex];
                    if (chest != null)
                    {
                        if (currentMode == UIMode.Protect)
                        {
                            // Protect mode: Verificar contraseña y confirmar coincidencia de contraseña
                            if (passwordInput.Text == confirmPasswordInput.Text)
                            {
                                if (!string.IsNullOrEmpty(passwordInput.Text))
                                {
                                    if (Main.netMode == NetmodeID.MultiplayerClient)
                                    {
                                        // Enviar solicitud al servidor
                                        ModPacket packet = ModContent.GetInstance<SafeChests>().GetPacket();
                                        packet.Write((byte)0);
                                        packet.Write(chest.x);
                                        packet.Write(chest.y);
                                        packet.Write(passwordInput.Text);
                                        packet.Write(false); // No estaba protegido
                                        packet.Send();
                                        // Actualizar el texto y posición
                                        protectButton.SetText("Desbloquear cofre");
                                        protectButton.Top.Pixels = 258; // Modo de desbloqueo
                                    }
                                    else
                                    {
                                        // Modo de un solo jugador o servidor
                                        ChestProtectionSystem.ToggleChestProtection(chest.x, chest.y, passwordInput.Text);
                                        protectButton.SetText("Desbloquear cofre");
                                        protectButton.Top.Pixels = 258; // Modo de desbloqueo
                                    }
                                    passwordInput.Text = "";
                                    confirmPasswordInput.Text = "";
                                    currentMode = UIMode.None;
                                }
                                else
                                {
                                    ChestProtectionSystem.SendMessageToAll("La contraseña no puede estar vacía.", Color.Red);
                                }
                            }
                            else
                            {
                                ChestProtectionSystem.SendMessageToAll("Las contraseñas no coinciden.", Color.Red);
                            }
                        }
                        else if (currentMode == UIMode.Unlock)
                        {
                            string storedPassword = ChestProtectionSystem.GetChestPassword(chest.x, chest.y);
                            if (unlockPasswordInput.Text == storedPassword)
                            {
                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                {
                                    // Enviar solicitud al servidor
                                    ModPacket packet = ModContent.GetInstance<SafeChests>().GetPacket();
                                    packet.Write((byte)0);
                                    packet.Write(chest.x);
                                    packet.Write(chest.y);
                                    packet.Write("");
                                    packet.Write(true); // Estaba protegido
                                    packet.Send();
                                    // Actualizar el texto y posición
                                    protectButton.SetText("Proteger cofre");
                                    protectButton.Top.Pixels = 428; // Posición original
                                }
                                else
                                {
                                    // Modo de un solo jugador o servidor
                                    ChestProtectionSystem.ToggleChestProtection(chest.x, chest.y, "");
                                    protectButton.SetText("Proteger cofre");
                                    protectButton.Top.Pixels = 428; // Posición original
                                }
                                unlockPasswordInput.Text = "";
                                currentMode = UIMode.None;
                            }
                            else
                            {
                                ChestProtectionSystem.SendMessageToAll("Contraseña incorrecta.", Color.Red);
                            }
                        }
                        UpdateUIElements();
                    }
                }
                else
                {
                    ChestProtectionSystem.SendMessageToAll("No hay ningún cofre abierto.", Color.Red);
                    currentMode = UIMode.None;
                    UpdateUIElements(); // Actualizar la UI del usuario dependiendo del modo
                }
            };

            // Agregar los elementos a la UI
            Append(protectButton);
            UpdateUIElements(); // Configurar visibilidad inicial
        }

        private void UpdateUIElements()
        {
            // Eliminar todos los elementos relacionados con la entrada de texto
            passwordLabel.Remove();
            passwordInput.Remove();
            confirmPasswordLabel.Remove();
            confirmPasswordInput.Remove();
            unlockPasswordLabel.Remove();
            unlockPasswordInput.Remove();
            acceptButton.Remove();

            // Añadir elementos en función del modo actual y ajustar posición de acceptButton
            if (currentMode == UIMode.Protect)
            {
                if (!Children.Contains(passwordLabel)) Append(passwordLabel);
                if (!Children.Contains(passwordInput)) Append(passwordInput);
                if (!Children.Contains(confirmPasswordLabel)) Append(confirmPasswordLabel);
                if (!Children.Contains(confirmPasswordInput)) Append(confirmPasswordInput);
                if (!Children.Contains(acceptButton)) Append(acceptButton);
                acceptButton.Top.Pixels = 470; // Posición original para modo Protect
            }
            else if (currentMode == UIMode.Unlock)
            {
                if (!Children.Contains(unlockPasswordLabel)) Append(unlockPasswordLabel);
                if (!Children.Contains(unlockPasswordInput)) Append(unlockPasswordInput);
                if (!Children.Contains(acceptButton)) Append(acceptButton);
                acceptButton.Top.Pixels = 300; // Modo de desbloqueo
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Detectar cambios en las entradas de texto
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

            // Comprueba si el cofre ha cambiado o el inventario está cerrado
            Player player = Main.player[Main.myPlayer];
            if (player.chest != lastChestIndex)
            {
                lastChestIndex = player.chest;
                if (player.chest != -1)
                {
                    Chest chest = Main.chest[player.chest];
                    if (chest != null)
                    {
                        bool isLocked = ChestProtectionSystem.IsChestProtected(chest.x, chest.y);
                        protectButton.SetText(isLocked ? "Desbloquear cofre" : "Proteger cofre");
                        protectButton.Top.Pixels = isLocked ? 258 : 428; // 258 para "Desbloquear", 428 para "Proteger"
                        currentMode = UIMode.None; // Siempre se restablece a None al cambiar de cofre
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
            if (player.chest != -1)
            {
                Chest chest = Main.chest[player.chest];
                if (chest != null)
                {
                    bool isLocked = ChestProtectionSystem.IsChestProtected(chest.x, chest.y);
                    protectButton.SetText(isLocked ? "Desbloquear cofre" : "Proteger cofre");
                    protectButton.Top.Pixels = isLocked ? 258 : 428; // 258 para "Desbloquear", 428 para "Proteger"
                    currentMode = UIMode.None; // Siempre se restablece a None al cambiar de cofre
                    UpdateUIElements();
                }
            }
            else
            {
                currentMode = UIMode.None;
                UpdateUIElements();
            }
        }

        // Actualizar el texto del botón
        public void UpdateButtonText(string text)
        {
            protectButton?.SetText(text);
            protectButton.Top.Pixels = text == "Desbloquear cofre" ? 258 : 428; // Ajustar posición según el texto
            currentMode = UIMode.None;
            UpdateUIElements();
        }
    }
}