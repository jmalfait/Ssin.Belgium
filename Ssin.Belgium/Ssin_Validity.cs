﻿using System;

namespace Ssin.Belgium
{
    public partial struct Ssin
    {
        public bool IsValid()
            => IsDatePartValid() && IsRegistrationIndexValid() && IsControlValid();

        private bool IsDatePartValid()
        {
            //Based on https://www.ibz.rrn.fgov.be/fileadmin/user_upload/fr/rn/instructions/liste-TI/TI000_Numerodidentification.pdf

            //Numéro BIS: le mois est simplement augmenté de 20 ou 40
            //Le mois peut être compris entre 0 et 12, entre 20 et 32 ou entre 40 et 52
            //Calculer mois réel
            if (Month > 52)
                return false;

            var month = Month;
            while (month > 12)
                month -= 20;
            
            //Si un nombre est négatif => invalide d'office
            if (Year < 0 || month < 0 || Day < 0)
                return false;
            
            //Date de naissance connue entièrement
            if (Year > 0 && month > 0 && Day > 0)
            {
                try
                {
                    var _ = new DateTime(Year, month, Day);
                    return true;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return false;
                }
            }

            //Date de naissance totalement inconnue : Year = 0, Month = 0, Day = 1
            if (Year == 0 && month == 0 && Day > 1)
                return true;

            //On connaît juste l'année ou juste l'année et le mois de naissance : Year > 0, Month = 0, Day >= 0
            if (Year > 0 && month == 0 & Day >= 0)
                return true;

            return false;
        }

        private bool IsRegistrationIndexValid()
        {
            //De 001 à 997 pour les hommes, de 002 à 998 pour les femmes 
            return RegistrationIndex > 0 && RegistrationIndex < 998;
        }

        private bool IsControlValid()
        {
            //Rule changes according to century of birth, but processes are inexpensive
            //so we can always compute them both instead of trying to guess which rule should be used
            var control19XX = ComputeControlFor19XX();
            var control20XX = ComputeControlFor20XX();

            return control20XX == Control || control19XX == Control;
        }

        private long ComputeControlFor19XX()
        {
            var composite = long.Parse($"{Year:D2}{Month:D2}{Day:D2}{RegistrationIndex:D3}");
            return ComputeControlFromComposite(composite);
        }

        private long ComputeControlFor20XX()
        {
            var composite = long.Parse($"2{Year:D2}{Month:D2}{Day:D2}{RegistrationIndex:D3}");
            return ComputeControlFromComposite(composite);
        }

        private static long ComputeControlFromComposite(long composite)
            => 97 - (composite % 97);


        public static bool IsValid(string ssin)
            => TryParse(ssin, out var parsed) && parsed.IsValid();

    }
}