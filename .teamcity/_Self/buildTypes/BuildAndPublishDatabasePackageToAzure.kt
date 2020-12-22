// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

package _self.buildTypes

import jetbrains.buildServer.configs.kotlin.v2019_2.*
import jetbrains.buildServer.configs.kotlin.v2019_2.buildFeatures.swabra
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.powerShell
import jetbrains.buildServer.configs.kotlin.v2019_2.triggers.finishBuildTrigger

object BuildAndPublishDatabasePackageToAzure : BuildType ({
    name = "Build and Publish Database Package to Azure"
    description = "Publishes NuGet package to the Azure feed"

    publishArtifacts = PublishMode.SUCCESSFUL

    option("shouldFailBuildOnAnyErrorMessage", "true")

    params {
        param("nuGet.packageFile", "placeholder")
        param("nuGet.packageVersion", "placeholder")
        param("env.VSS_NUGET_EXTERNAL_FEED_ENDPOINTS", "{\"endpointCredentials\": [{\"endpoint\": \"%azureArtifacts.feed.nuget%\",\"username\": \"%azureArtifacts.edFiBuildAgent.userName%\",\"password\": \"%azureArtifacts.edFiBuildAgent.accessToken%\"}]}")
    }

    vcs {
        root(DslContext.settingsRoot)
    }  

    steps {
         powerShell {
            name = "Create NuGet Package"
            formatStderrAsError = true
            executionMode = BuildStep.ExecutionMode.RUN_ON_SUCCESS
            scriptMode = script {
                content = """
                    .\build.ps1 -Version %adminApp.version% -BuildCounter %build.counter% -Command PackageDatabaseScripts -Configuration Release
                """.trimIndent()
            }
        }
        powerShell {
            name = "Lookup Package Name and Version"
            formatStderrAsError = true
            executionMode = BuildStep.ExecutionMode.RUN_ON_SUCCESS
            scriptMode = file {
                path = """eng\teamcity-get-package-version.ps1 -packageType Database"""
            }
        }
        powerShell {
            name = "Publish to Azure"
            formatStderrAsError = true
            executionMode = BuildStep.ExecutionMode.RUN_ON_SUCCESS
            scriptMode = script {
                content = """
                    ${'$'}arguments = @{
                    	PackageFile = "%nuGet.packageFile%"
                        EdFiNuGetFeed = "%azureArtifacts.feed.nuget%"
                        NuGetApiKey = "%azureArtifacts.edFiBuildAgent.accessToken%"
                    }
                    ./build.ps1 Push @arguments
                """.trimIndent()
            }
        }
    }
  triggers {
        finishBuildTrigger {
            buildType = "${BuildBranch.id}"
        }
    }

    features {
        swabra {
            forceCleanCheckout = true
        }
    }

    dependencies {
        dependency(BuildBranch) {
            snapshot {
            }

            artifacts {
                artifactRules = """
                    +:*pre*.nupkg =>
                """.trimIndent()
            }
        }
    }
})
