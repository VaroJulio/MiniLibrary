import { useCallback, useRef } from 'react';

const MUTE_KEY = 'minilib_notification_sound_muted';

/**
 * Generates a short pleasant notification tone using the Web Audio API.
 * No external audio file is needed — the sound is synthesized on-the-fly.
 */
function playNotificationTone(): void {
  try {
    const ctx = new (window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext)();
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

    // Clean up audio context after playback
    oscillator.onended = () => {
      ctx.close();
    };
  } catch {
    // Silently fail if Web Audio API is not available
  }
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
      playNotificationTone();
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
