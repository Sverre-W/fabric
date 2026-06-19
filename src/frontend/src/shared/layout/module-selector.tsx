import { Link, useLocation } from '@tanstack/react-router';
import { ChevronDown } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';

import { appModules, getModuleByPathname } from '@/shared/modules/app-modules';
import { cn } from '@/shared/utils/cn';

export function ModuleSelector() {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const location = useLocation();
  const activeModule = getModuleByPathname(location.pathname);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    function handlePointerDown(event: PointerEvent) {
      if (!containerRef.current?.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setIsOpen(false);
      }
    }

    document.addEventListener('pointerdown', handlePointerDown);
    document.addEventListener('keydown', handleKeyDown);

    return () => {
      document.removeEventListener('pointerdown', handlePointerDown);
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen]);

  return (
    <div ref={containerRef} className="relative min-w-0">
      <button
        type="button"
        className="flex h-12 w-full min-w-0 items-center justify-between gap-3 rounded-interactive border border-border bg-content px-3 text-left text-[14px] font-medium transition hover:bg-hover-blue"
        aria-haspopup="menu"
        aria-expanded={isOpen}
        onClick={() => setIsOpen((current) => !current)}
      >
        <span className={cn('truncate', !activeModule && 'text-muted-foreground')}>{activeModule?.name ?? 'Select module'}</span>
        <ChevronDown className={cn('size-4 shrink-0 text-muted-foreground transition', isOpen && 'rotate-180')} />
      </button>

      {isOpen ? (
        <div className="absolute left-0 right-0 top-full z-20 mt-2 overflow-hidden rounded-structural border border-border bg-content" role="menu">
          <div className="p-2">
            {appModules.map((module) => {
              const Logo = module.logo;
              const isActive = activeModule?.id === module.id;

              return (
                <Link
                  key={module.id}
                  to={module.to}
                  className={cn(
                    'flex gap-3 rounded-interactive p-3 text-left transition hover:bg-hover-blue',
                    isActive && 'bg-active-blue text-foreground hover:bg-active-blue',
                  )}
                  onClick={() => setIsOpen(false)}
                  role="menuitem"
                >
                  <span className="flex size-10 shrink-0 items-center justify-center rounded-interactive bg-active-blue text-primary">
                    <Logo className="size-5" />
                  </span>
                  <span className="min-w-0">
                    <span className="block font-semibold">{module.name}</span>
                    <span className="mt-1 block text-[14px] leading-5 text-muted-foreground">{module.description}</span>
                  </span>
                </Link>
              );
            })}
          </div>
        </div>
      ) : null}
    </div>
  );
}
