import { useEffect, useRef, useState } from 'react';
import { useUnreadCount } from './useNotifications';
import { useNotificationSound } from './useNotificationSound';

const ANIMATION_DURATION_MS = 1800; // 0.6s × 3 iterations

/**
 * Detects when the unread notification count increases between polls.
 * On increase: plays a notification sound and triggers bell animation.
 *
 * Does NOT trigger on initial page load — only on delta increases.
 *
 * Returns { unreadCount, animate } for the NotificationBell component.
 */
export function useNewNotificationDetector() {
  const unreadCount = useUnreadCount();
  const { play } = useNotificationSound();
  const previousCountRef = useRef<number | null>(null);
  const [animate, setAnimate] = useState(false);

  useEffect(() => {
    // On first render, store the initial count without triggering effects
    if (previousCountRef.current === null) {
      previousCountRef.current = unreadCount;
      return;
    }

    // Only trigger when count increases (new notifications arrived)
    if (unreadCount > previousCountRef.current) {
      play();
      setAnimate(true);

      // Reset animation after it completes
      const timer = setTimeout(() => {
        setAnimate(false);
      }, ANIMATION_DURATION_MS);

      previousCountRef.current = unreadCount;
      return () => clearTimeout(timer);
    }

    // Update ref for decreases (e.g., notifications read)
    previousCountRef.current = unreadCount;
  }, [unreadCount, play]);

  return { unreadCount, animate };
}
