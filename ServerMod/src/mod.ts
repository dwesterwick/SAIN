/* eslint-disable prefer-const */
/* eslint-disable @typescript-eslint/brace-style */

import { ConfigTypes } from "@spt/models/enums/ConfigTypes";
import type { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod";
import type { IPmcConfig } from "@spt/models/spt/config/IPmcConfig";
import type { IBotConfig } from "@spt/models/spt/config/IBotConfig";
import type { ConfigServer } from "@spt/servers/ConfigServer";
import type { DatabaseServer } from "@spt/servers/DatabaseServer";
import type { DependencyContainer } from "tsyringe";

let botConfig: IBotConfig;
let pmcConfig: IPmcConfig;
let configServer: ConfigServer;

class SAIN implements IPostDBLoadMod {
    public postDBLoad(container: DependencyContainer): void {
        configServer = container.resolve<ConfigServer>("ConfigServer");
        pmcConfig = configServer.getConfig<IPmcConfig>(ConfigTypes.PMC);
        botConfig = configServer.getConfig<IBotConfig>(ConfigTypes.BOT);
        const databaseServer = container.resolve<DatabaseServer>("DatabaseServer");
        const tables = databaseServer.getTables();

        // Only allow `pmcBEAR` and `pmcUSEC` brains to spawn for PMCs
        for (const pmcType in pmcConfig.pmcType)
        {
            for (const map in pmcConfig.pmcType[pmcType])
            {
                const pmcBrains = pmcConfig.pmcType[pmcType][map]
                for (const brain in pmcBrains)
                {
                    pmcBrains[brain] = 0;
                }
				
                pmcBrains.pmcBot = 1;
				//pmcBrains.pmcBEAR = 1;
				//pmcBrains.pmcUSEC = 1;
            }
        }

        // Only allow `assault` brains for scavs
        for (const map in botConfig.assaultBrainType)
        {
            const scavBrains = botConfig.assaultBrainType[map];
            for (const brain in scavBrains)
            {
                scavBrains[brain] = 0;
            }
			
			scavBrains.assault = 1;
        }

        // Only allow `pmcBEAR` and `pmcUSEC` brains for player scavs
        for (const map in botConfig.playerScavBrainType)
        {
            const playerScavBrains = botConfig.playerScavBrainType[map];
            for (const brain in playerScavBrains)
            {
                playerScavBrains[brain] = 0;
            }
			
            playerScavBrains.pmcBot = 1;
			//playerScavBrains.pmcBEAR = 1;
			//playerScavBrains.pmcUSEC = 1;
        }

        for (const locationName in tables.locations)
        {
            const location = tables.locations[locationName].base;

            // biome-ignore lint/complexity/useOptionalChain: <explanation>
            if (location && location.BotLocationModifier)
            {
                location.BotLocationModifier.AccuracySpeed = 1;
                location.BotLocationModifier.GainSight = 1;
                location.BotLocationModifier.Scattering = 1;
                location.BotLocationModifier.VisibleDistance = 1;
            }
        }
    }
}
module.exports = { mod: new SAIN() }