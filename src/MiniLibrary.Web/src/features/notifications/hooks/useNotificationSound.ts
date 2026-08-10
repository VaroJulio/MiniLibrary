import { useCallback, useRef } from 'react';

const MUTE_KEY = 'minilib_notification_sound_muted';

// Shared AudioContext — created once and reused across plays.
// Must be resumed after a user gesture; we resume on the first call to play().
let sharedAudioCtx: AudioContext | null = null;

function getAudioContext(): AudioContext | null {
  try {
    if (!sharedAudioCtx || sharedAudioCtx.state === 'closed') {
      sharedAudioCtx = new (window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext)();
    }
    return sharedAudioCtx;
  } catch {
    return null;
  }
}

/**
 * Generates a short pleasant notification tone using the Web Audio API.
 * No external audio file is needed — the sound is synthesized on-the-fly.
 */
async function playNotificationTone(): Promise<void> {
  try {
    const ctx = getAudioContext();
    if (!ctx) return;

    // Resume the context if suspended (browser autoplay policy)
    if (ctx.state === 'suspended') {
      await ctx.resume();
    }

    const oscillator = ctx.createOscillator();
    const gainNode = ctx.createGain();

    oscillator.connect(gainNode);
    gainNode.connect(ctx.destination);

    // Two-tone notification: a short ascending pair of notes
    oscillator.type = 'sine';
    oscillator.frequency.setValueAtTime(587.33, ctx.currentTime); // D5
    oscillator.frequency.setValueAtTime(783.99, ctx.currentTime + 0.1); // G5

    // Fade in/out for a pleasant sound
    gainNode.gain.setValueAtTime(0, ctx.currentTime);
    gainNode.gain.linearRampToValueAtTime(0.3, ctx.currentTime + 0.02);
    gainNode.gain.setValueAtTime(0.3, ctx.currentTime + 0.1);
    gainNode.gain.linearRampToValueAtTime(0.3, ctx.currentTime + 0.12);
    gainNode.gain.linearRampToValueAtTime(0, ctx.currentTime + 0.25);

    oscillator.start(ctx.currentTime);
    oscillator.stop(ctx.currentTime + 0.25);
  } catch {
    // Silently fail if Web Audio API is not available
  }
}

// Resume the shared AudioContext on the first user interaction.
// This "unlocks" audio for all subsequent programmatic plays.
if (typeof window !== 'undefined') {
  const unlockAudio = () => {
    const ctx = getAudioContext();
    if (ctx && ctx.state === 'suspended') {
      ctx.resume();
    }
    window.removeEventListener('click', unlockAudio);
    window.removeEventListener('keydown', unlockAudio);
    window.removeEventListener('touchstart', unlockAudio);
  };
  window.addEventListener('click', unlockAudio, { once: true });
  window.addEventListener('keydown', unlockAudio, { once: true });
  window.addEventListener('touchstart', unlockAudio, { once: true });
}

export function isSoundMuted(): boolean {
  try {
    return localStorage.getItem(MUTE_KEY) === 'true';
  } catch {
    return false;
  }
}

export function setSoundMuted(muted: boolean): void {
  try {
    localStorage.setItem(MUTE_KEY, String(muted));
  } catch {
    // localStorage unavailable — ignore
  }
}

/**
 * Hook that provides a play function for notification sounds.
 * Respects the user's mute preference stored in localStorage.
 * Returns { play, isMuted, toggleMute }.
 */
export function useNotificationSound() {
  const isMutedRef = useRef(isSoundMuted());

  const play = useCallback(() => {
    // Re-read mute state at play time (in case toggled elsewhere)
    isMutedRef.current = isSoundMuted();
    if (!isMutedRef.current) {
      void playNotificationTone();
    }
  }, []);

  const toggleMute = useCallback(() => {
    const newValue = !isSoundMuted();
    setSoundMuted(newValue);
    isMutedRef.current = newValue;
    return newValue;
  }, []);

  return { play, isMuted: isMutedRef.current, toggleMute };
}
